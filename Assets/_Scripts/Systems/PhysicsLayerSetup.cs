using UnityEngine;

/// <summary>
/// PhysicsLayerSetup - Configura la matriz de colisiones de Physics2D al iniciar.
/// Garantiza que Player(9) detecte a Enemy(10) y viceversa en todas las queries.
/// Se ejecuta automaticamente antes de la primera escena.
/// </summary>
public class PhysicsLayerSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void SetupLayers()
    {
        // Asegurar que todos los layers relevantes pueden interactuar
        Physics2D.IgnoreLayerCollision(9,  10, false); // Player  <-> Enemy
        Physics2D.IgnoreLayerCollision(9,  11, false); // Player  <-> Projectile
        Physics2D.IgnoreLayerCollision(10, 11, false); // Enemy   <-> Projectile
        Physics2D.IgnoreLayerCollision(8,   9, false); // Ground  <-> Player
        Physics2D.IgnoreLayerCollision(8,  10, false); // Ground  <-> Enemy
        Physics2D.IgnoreLayerCollision(10, 10, true);  // Enemy   <-> Enemy (no se empujan)
        Physics2D.IgnoreLayerCollision(11, 11, true);  // Proj    <-> Proj (no se chocan)
    }
}
