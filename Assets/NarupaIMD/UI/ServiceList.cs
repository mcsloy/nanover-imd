using System;
using System.Linq;
using Essd;
using Narupa.Frontend.UI;
using NarupaIMD;
using TMPro;
using UnityEngine;

public class ServiceList : MonoBehaviour
{
    [SerializeField]
    private UiButton prefab;

    [SerializeField]
    private ServiceDiscovery services;

    private void OnEnable()
    {
        services.ServiceDiscovered += ServicesOnServiceDiscovered;
        foreach (var service in services.Services)
            ServicesOnServiceDiscovered(service);
    }

    private void OnDisable()
    {
        services.ServiceDiscovered -= ServicesOnServiceDiscovered;
    }

    private void ServicesOnServiceDiscovered(ServiceHub obj)
    {
        var button = Instantiate(prefab, transform);
        button.gameObject.SetActive(true);
        button.Text = obj.Name;
        button.OnClick += () => services.Connect(obj);
        button.Subtext = obj.Address;
    }
}