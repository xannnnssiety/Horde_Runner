using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ManualRootRotationHandler : MonoBehaviour
{
    private Animator animator;

    // Лучше кешировать хеш тега для производительности, чем сравнивать строки каждый кадр.
    private readonly int manualRotationTagHash = Animator.StringToHash("ManualRootRotation");

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Этот метод вызывается каждый кадр, когда аниматор вычисляет root motion.
    // Он дает нам контроль над тем, как это движение применяется.
    void OnAnimatorMove()
    {
        if (animator == null) return;

        // Проверяем, активен ли сейчас стейт с нашим тегом на базовом слое (0)
        if (animator.GetCurrentAnimatorStateInfo(0).tagHash == manualRotationTagHash)
        {
            // Если да, то мы вручную применяем вращение из анимации (deltaRotation)
            // к transform нашего объекта.
            // Оператор *= для кватернионов означает "добавить вращение".
            transform.rotation *= animator.deltaRotation;

            // Важно! Также стоит применить и deltaPosition.
            // Даже если анимация "на месте", могут быть микро-смещения,
            // которые без этой строки вызовут "топтание" на месте.
            // Если твоя анимация идеально на месте, эту строку можно закомментировать.
            transform.position += animator.deltaPosition;
        }
        // Если проигрывается любая другая анимация (без нашего тега),
        // этот код не выполнится, и персонаж будет управляться как обычно
        // (например, твоим скриптом Character Controller'а).
    }
}