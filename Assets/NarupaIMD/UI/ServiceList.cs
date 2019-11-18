using System.Linq;
using Essd;
using Narupa.Frontend.UI;
using NarupaIMD;
using TMPro;
using UnityEngine;

public class ServiceList : MonoBehaviour
{
    [SerializeField]
    private NarupaButton prefab;

    [SerializeField]
    private ServiceDiscovery services;

    private void OnEnable()
    {
        services.ServiceDiscovered += ServicesOnServiceDiscovered;
        foreach (var service in services.Services)
            ServicesOnServiceDiscovered(service);
    }

    private void ServicesOnServiceDiscovered(ServiceHub obj)
    {
        var button = Instantiate(prefab, transform);
        button.gameObject.SetActive(true);
        button.Text = obj.Name;
        button.OnClick += () => services.Connect(obj);
        button.transform.GetComponentsInChildren<TMP_Text>()
              .FirstOrDefault(t => t.name.Contains("Address")).text = obj.Address;
    }
}