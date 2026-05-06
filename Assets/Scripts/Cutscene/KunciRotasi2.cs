using UnityEngine;

public class KunciRotasi2 : MonoBehaviour
{
    // Update dijalankan setiap frame, jadi rotasi bakal dipaksa terus
    void Update()
    {
        // Angka 180 ini buat maksa hadap kiri. Kalau kurang pas, ganti angkanya.
        transform.rotation = Quaternion.Euler(0, 180, 0);
    }
}
