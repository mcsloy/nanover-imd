using System;
using System.Collections;
using System.Collections.Generic;
using Essd;
using Narupa.Frontend.UI;
using NarupaIMD;
using UnityEngine;

public class ServiceList : MonoBehaviour
{
    [SerializeField]
    private NarupaButton prefab;

    [SerializeField]
    private ServiceDiscovery services;

    private void Awake()
    {
        services.ServiceDiscovered += ServicesOnServiceDiscovered;
    }

    private void ServicesOnServiceDiscovered(ServiceHub obj)
    {
        var button = Instantiate(prefab, transform);
        button.gameObject.SetActive(true);
        button.Text = obj.Name;
        button.OnClick += () => services.Connect(obj);
    }
}
