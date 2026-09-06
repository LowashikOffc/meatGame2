using UnityEngine;

public class ElevatorCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Проверяем, что объект касается лифта сверху (стоит на платформе)
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 normal = contact.normal;

            // Если нормаль направлена вверх - объект стоит на лифте
            if (normal.y > 0.5f)
            {
                // Делаем объект ребенком лифта
                collision.transform.SetParent(transform);
                Debug.Log(collision.gameObject.name + " теперь на лифте!");
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Когда объект сходит с лифта - открепляем
        if (collision.transform.IsChildOf(transform))
        {
            collision.transform.SetParent(null);
            Debug.Log(collision.gameObject.name + " покинул лифт!");
        }
    }
}
