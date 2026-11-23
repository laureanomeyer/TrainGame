using System;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_WEBGL
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
#else
using Firebase;
using Firebase.Auth;
using Firebase.Database;
#endif

// =========================================================
//  BACKEND DE RED
//  - WebGL            -> API REST de Firebase (UnityWebRequest)
//  - Resto de plataformas -> Firebase Unity SDK
//  El AnalyticsManager solo conoce esta interfaz.
// =========================================================

public interface IAnalyticsBackend
{
    bool IsReady { get; }

    /// <summary>Uid anónimo del equipo (se guarda como ownerUid).</summary>
    string Uid { get; }

    /// <summary>Conecta con Firebase y hace el login anónimo.</summary>
    Task<bool> InitAsync();

    /// <summary>Pide el siguiente ID único. Devuelve 0 si falló.</summary>
    Task<int> GetNextUserIdAsync();

    /// <summary>Sube el JSON de un usuario a users/{userId}.</summary>
    Task<bool> SetUserJsonAsync(int userId, string json);
}

#if UNITY_WEBGL

// =========================================================
//  WEBGL: API REST
// =========================================================

public class FirebaseRestBackend : IAnalyticsBackend
{
    private readonly string apiKey;
    private readonly string dbUrl;

    private string idToken;
    private string refreshToken;
    private string uid;
    private DateTime expiresAtUtc;

    public bool IsReady => !string.IsNullOrEmpty(idToken);
    public string Uid => uid;

    public FirebaseRestBackend(string apiKey, string databaseUrl)
    {
        this.apiKey = apiKey;
        this.dbUrl = (databaseUrl ?? "").TrimEnd('/');
    }

    // ----------------- Respuestas -----------------

    private struct Response
    {
        public bool ok;
        public long code;
        public string body;
        public string etag;
    }

    [Serializable]
    private class SignUpResponse
    {
        public string idToken;
        public string refreshToken;
        public string expiresIn;
        public string localId;
    }

    [Serializable]
    private class RefreshResponse
    {
        public string id_token;
        public string refresh_token;
        public string expires_in;
        public string user_id;
    }

    // ----------------- Login anónimo -----------------

    public async Task<bool> InitAsync()
    {
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(dbUrl))
        {
            Debug.LogError("[Analytics] WebGL: faltan Web Api Key o Database Url en el Inspector.");
            return false;
        }

        Response res = await SendAsync(
            "POST",
            $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={apiKey}",
            "{\"returnSecureToken\":true}");

        if (!res.ok)
        {
            Debug.LogError($"[Analytics] Login anónimo falló ({res.code}): {res.body}");
            return false;
        }

        SignUpResponse data = JsonUtility.FromJson<SignUpResponse>(res.body);
        idToken = data.idToken;
        refreshToken = data.refreshToken;
        uid = data.localId;
        expiresAtUtc = DateTime.UtcNow.AddSeconds(double.Parse(data.expiresIn));

        return true;
    }

    // El token dura 1 hora: se renueva solo, manteniendo el mismo uid.
    private async Task<bool> EnsureTokenAsync()
    {
        if (DateTime.UtcNow < expiresAtUtc.AddSeconds(-60)) return true;

        Response res = await SendAsync(
            "POST",
            $"https://securetoken.googleapis.com/v1/token?key={apiKey}",
            $"grant_type=refresh_token&refresh_token={refreshToken}",
            "application/x-www-form-urlencoded");

        if (!res.ok)
        {
            Debug.LogWarning($"[Analytics] No se pudo renovar el token ({res.code}): {res.body}");
            return false;
        }

        RefreshResponse data = JsonUtility.FromJson<RefreshResponse>(res.body);
        idToken = data.id_token;
        refreshToken = data.refresh_token;
        expiresAtUtc = DateTime.UtcNow.AddSeconds(double.Parse(data.expires_in));

        return true;
    }

    // ----------------- ID único (equivale a la transacción del SDK) -----------------

    public async Task<int> GetNextUserIdAsync()
    {
        if (!await EnsureTokenAsync()) return 0;

        string url = $"{dbUrl}/analytics/meta/nextUserId.json?auth={idToken}";

        // Se reintenta si otro cliente cambió el contador entre la lectura y la escritura.
        for (int attempt = 0; attempt < 5; attempt++)
        {
            Response get = await SendAsync("GET", url, null, null,
                new Dictionary<string, string> { { "X-Firebase-ETag", "true" } });

            if (!get.ok)
            {
                Debug.LogWarning($"[Analytics] No se pudo leer el contador ({get.code}): {get.body}");
                return 0;
            }

            if (string.IsNullOrEmpty(get.etag))
            {
                Debug.LogWarning("[Analytics] La respuesta no trae ETag (el navegador no lo expone). No se puede asignar un ID seguro.");
                return 0;
            }

            string raw = get.body.Trim();
            long current = raw == "null" || raw.Length == 0 ? 0 : long.Parse(raw);

            // Solo escribe si el contador sigue igual que cuando lo leímos.
            Response put = await SendAsync("PUT", url, (current + 1).ToString(), null,
                new Dictionary<string, string> { { "if-match", get.etag } });

            if (put.ok) return (int)(current + 1);

            if (put.code == 412) continue; // otro cliente se adelantó: reintentar

            Debug.LogWarning($"[Analytics] No se pudo actualizar el contador ({put.code}): {put.body}");
            return 0;
        }

        return 0;
    }

    // ----------------- Subir usuario -----------------

    public async Task<bool> SetUserJsonAsync(int userId, string json)
    {
        if (!await EnsureTokenAsync()) return false;

        Response res = await SendAsync("PUT", $"{dbUrl}/users/{userId}.json?auth={idToken}", json);

        if (!res.ok)
            Debug.LogWarning($"[Analytics] No se pudo subir el usuario {userId} ({res.code}): {res.body}");

        return res.ok;
    }

    // ----------------- HTTP -----------------

    private static Task<Response> SendAsync(
        string method, string url, string body,
        string contentType = "application/json",
        Dictionary<string, string> headers = null)
    {
        TaskCompletionSource<Response> tcs = new TaskCompletionSource<Response>();

        UnityWebRequest request = new UnityWebRequest(url, method);
        request.downloadHandler = new DownloadHandlerBuffer();

        if (body != null)
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            request.SetRequestHeader("Content-Type", contentType ?? "application/json");
        }

        if (headers != null)
        {
            foreach (KeyValuePair<string, string> header in headers)
                request.SetRequestHeader(header.Key, header.Value);
        }

        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        operation.completed += _ =>
        {
            Response response = new Response
            {
                ok = request.result == UnityWebRequest.Result.Success,
                code = request.responseCode,
                body = request.downloadHandler.text,
                etag = request.GetResponseHeader("ETag")
            };

            request.Dispose();
            tcs.SetResult(response);
        };

        return tcs.Task;
    }
}

#else

// =========================================================
//  RESTO DE PLATAFORMAS: FIREBASE UNITY SDK
// =========================================================

public class FirebaseSdkBackend : IAnalyticsBackend
{
    private readonly string databaseUrl;
    private DatabaseReference root;
    private string uid;
    private bool ready;

    public bool IsReady => ready;
    public string Uid => uid;

    public FirebaseSdkBackend(string databaseUrl)
    {
        this.databaseUrl = databaseUrl;
    }

    public async Task<bool> InitAsync()
    {
        DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (status != DependencyStatus.Available)
        {
            Debug.LogError($"[Analytics] Firebase no disponible: {status}");
            return false;
        }

        AuthResult result = await FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync();
        uid = result.User.UserId;

        FirebaseDatabase database = string.IsNullOrEmpty(databaseUrl)
            ? FirebaseDatabase.DefaultInstance
            : FirebaseDatabase.GetInstance(databaseUrl);

        root = database.RootReference;
        ready = true;

        return true;
    }

    public async Task<int> GetNextUserIdAsync()
    {
        try
        {
            DataSnapshot result = await root.Child("analytics").Child("meta").Child("nextUserId")
                .RunTransaction(data =>
                {
                    long current = data.Value != null ? Convert.ToInt64(data.Value) : 0;
                    data.Value = current + 1;
                    return TransactionResult.Success(data);
                });

            return Convert.ToInt32(result.Value);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Analytics] No se pudo asignar un ID: {e.Message}");
            return 0;
        }
    }

    public async Task<bool> SetUserJsonAsync(int userId, string json)
    {
        try
        {
            await root.Child("users").Child(userId.ToString()).SetRawJsonValueAsync(json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Analytics] No se pudo subir el usuario {userId}: {e.Message}");
            return false;
        }
    }
}

#endif