using UnityEngine;

public class CharacterAnimationEvent : MonoBehaviour
{
    [SerializeField] private Character character;

    // Gắn hàm này vào đúng frame animation cần gây damage hoặc kích hoạt hiệu ứng skill.
    public void ExecuteAttack()
    {
        character.Combat.ExecuteAttack();
    }

    // Gắn hàm này vào frame kết thúc của animation attack trong Animation Event.
    public void EndAttack()
    {
        character.Combat.EndAttack();
    }

    public void DespawnCharacter()
    {
        character.Despawn();
    }
}
