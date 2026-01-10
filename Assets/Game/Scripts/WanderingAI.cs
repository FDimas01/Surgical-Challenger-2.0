using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class WanderingAI : MonoBehaviour
{
    public float speed = 2f;
    public float changeDirectionTime = 2f; // Tempo para mudar de direção
    public float waitTime = 1f; // Tempo parado

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 movement;
    private float timer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = 0; // Garante que não caia
        rb.freezeRotation = true; // Não deixa girar
        PickNewDirection();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            StartCoroutine(ChangeDirectionRoutine());
            timer = changeDirectionTime + waitTime; // Reseta o timer
        }

        // Atualiza Animação
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // Move o personagem usando Física (para colidir com paredes)
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    void PickNewDirection()
    {
        // Guarda a direção antiga
        Vector2 oldDirection = movement;
        
        // Tenta escolher uma direção nova até que seja DIFERENTE da antiga
        // O loop 'do-while' garante que ele não repita a mesma direção em que bateu
        do 
        {
            float x = Mathf.RoundToInt(Random.Range(-1f, 1f));
            float y = Mathf.RoundToInt(Random.Range(-1f, 1f));
            movement = new Vector2(x, y);

            // Evita ficar parado (0,0)
            if (movement == Vector2.zero) movement = new Vector2(1, 0);

        } while (movement == oldDirection); // Repete se for igual à anterior
    }

    IEnumerator ChangeDirectionRoutine()
    {
        // 1. Para o movimento
        Vector2 savedMovement = movement;
        movement = Vector2.zero;
        
        // 2. Espera um pouco (Idle)
        yield return new WaitForSeconds(waitTime);

        // 3. Escolhe nova direção
        PickNewDirection();
    }

    void UpdateAnimation()
    {
        // Aqui configuramos as variáveis do Animator
        // Você precisa ter criado parametros "Horizontal", "Vertical" e "Speed" no Animator
        anim.SetFloat("Horizontal", movement.x);
        anim.SetFloat("Vertical", movement.y);
        anim.SetFloat("Speed", movement.sqrMagnitude);
    }
    
// --- NOVA FUNÇÃO PARA DETECTAR COLISÃO ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Se bater em qualquer coisa (paredes, móveis), muda de direção na hora!
        
        // 1. Escolhe uma nova direção
        PickNewDirection();

        // 2. Reseta o timer para ele não mudar de novo muito rápido
        // (Dá a ele um tempo cheio para andar na nova direção)
        timer = changeDirectionTime;
    }
}