using UnityEngine;

public class ShootProjectile : MonoBehaviour
{
    public bool input;
    public float attackInterval = 0.5f;
    public float shootForce = 200f;
    public Rigidbody projectile;
    public Vector3 direction = Vector3.forward;

    private float attackCooldown;

    private void Update()
    {
        float delta = Time.deltaTime;
        Process(delta);
    }

    private void OnDrawGizmosSelected()
    {
        EditorGizmos.color = Color.red;
        Vector3 direction = transform.TransformDirection(this.direction.normalized);
        EditorGizmos.DrawRay(transform.position, direction);
        EditorGizmos.ArrowHandleCap(0, transform.position + direction, Quaternion.LookRotation(direction), 0.2f);
    }

    public void Process(float delta)
    {
        attackCooldown -= delta;
        if (input && attackCooldown <= 0f)
        {
            attackCooldown = attackInterval;
            if (projectile != null)
            {
                Vector3 direction = transform.TransformDirection(this.direction.normalized);
                Rigidbody newProjectile = Instantiate(projectile, transform.position, transform.rotation);
                newProjectile.AddForce(direction * shootForce);
            }
        }
    }
}