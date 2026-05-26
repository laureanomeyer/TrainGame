using System;
using UnityEngine;

public class ActiveLocomotiveUI : MonoBehaviour
{
    [SerializeField] private GameObject UILocomotivesUpgrades;

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            UILocomotivesUpgrades.SetActive(false);
        }
    }
}
