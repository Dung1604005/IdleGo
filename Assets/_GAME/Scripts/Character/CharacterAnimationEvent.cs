using UnityEngine;

public class CharacterAnimationEvent : MonoBehaviour
{
    [SerializeField] private Character character;

    // Gắn hàm này vào đúng frame animation cần gây damage hoặc kích hoạt hiệu ứng skill.
    public void ExecuteAttack()
    {
        if (character != null)
        {
            character.Combat.ExecuteAttack();
        }
    }

    // Gắn hàm này vào frame kết thúc của animation attack trong Animation Event.
    public void EndAttack()
    {
        if (character != null)
        {
            character.Combat.EndAttack();
        }
    }
}
