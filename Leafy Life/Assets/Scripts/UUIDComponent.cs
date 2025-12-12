using UnityEngine;
using System;

public class UUIDComponent : MonoBehaviour {
    public string UUID { get; private set; }

    public string generateUUID() {
        UUID = Guid.NewGuid().ToString();
        print(gameObject.name + " uuid " + UUID);

        return UUID;
    }

    public void setUUID(string uuid) {
        UUID = uuid;
    }
}