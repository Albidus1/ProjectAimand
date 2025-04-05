using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class PlayerController : MonoBehaviour
{
    public float magnetRange = 10f; //자력 감지 최대 거리
    public float pullForce = 30f; //기본 자력 세기
    private bool isNorthPole = true; //플레이어 극성 (true: N극, false: S극)
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    void Update()
    {
        //D 키를 누르면 극성 변경
        if (Input.GetKeyDown(KeyCode.D))
        {
            isNorthPole = !isNorthPole;
            UpdateColor();
        }

        //S 키를 누르면 자력 사용
        if (Input.GetKey(KeyCode.S))
        {
            PullMagnet();
        }
    }

    void UpdateColor()
    {
        spriteRenderer.color = isNorthPole ? Color.red : Color.blue;
    }

    void PullMagnet()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, magnetRange);

        foreach (Collider2D col in colliders)
        {
            if (col.gameObject == this.gameObject) continue;

            NorthPole north = col.GetComponent<NorthPole>();
            SouthPole south = col.GetComponent<SouthPole>();
            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            Vector2 direction = (transform.position - col.transform.position); //transform.position: 플레이어 현재위치, col.transform.position: 오브젝트의 위치
            float distance = direction.magnitude;

            if (distance < 0.3f) continue; // 화면이탈 방지

            // 가까울수록 힘의 세기가 세짐
            float forceMagnet = pullForce / (distance * distance); // 거리 제곱에 반비례
            forceMagnet = Mathf.Clamp(forceMagnet, 0f, 50f); // 최대 힘 제한

            Vector2 force = direction * forceMagnet;

            if ((north != null && isNorthPole) || (south != null && !isNorthPole)) //같은 극
            {
                rb.AddForce(force, ForceMode2D.Force);
            }
            else if ((north != null && !isNorthPole) || (south != null && isNorthPole))// 다른 극
            {
                rb.AddForce(-force, ForceMode2D.Force);
            }

        }
    }
}