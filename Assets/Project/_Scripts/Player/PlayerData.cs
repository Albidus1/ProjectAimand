using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "플레이어 데이터")]
public class PlayerData : ScriptableObject
{
    [Header("중력")]
    [HideInInspector] public float gravity_strength;
    [HideInInspector] public float gravity_scale;
    [Space(5)]

    public float fall_gravity_mult;
    public float max_fall_speed;
    [Space(5)]

    public float fast_fall_gravity_mult;
    public float max_fast_fall_speed;
    [Space(20)]

    [Header("이동")]
    public float run_max_speed;
    public float run_acceleration;
    [HideInInspector] public float run_accel_amount;
    public float run_decceleration;
    [HideInInspector] public float run_deccel_amount;
    [Space(5)]

    [Range(0f, 1f)] public float accel_in_air;
    [Range(0f, 1f)] public float deccel_in_air;
    [Space(5)]

    public bool doConserveMomentum = true;
    [Space(20)]

    [Header("점프")]
    public float jump_height;
    public float jump_time_to_apex;
    [HideInInspector] public float jump_force;
    [Space(5)]

    public float jump_cut_gravity_mult;
    [Range(0f, 1.0f)] public float jump_hang_gravity_mult;
    public float jump_hang_time_threshold;
    public float jump_hang_acceleration_mult;
    public float jump_hang_max_speed_mult;
    [Space(20)]

    [Header("벽 점프")]
    public Vector2 wall_jump_force;
    [Space(5)]

    [Range(0f, 1f)] public float wall_jump_run_lerp;
    [Range(0f, 1.5f)] public float wall_jump_time;
    public bool do_turn_on_wall_jump;
    [Space(20)]

    [Header("슬라이드")]
    public float slide_speed;
    public float slide_accel;
    [Space(20)]

    [Header("벽 붙잡기")]
    public float grab_stamina;
    public float climb_up_speed;

    [Header("대쉬")]
    public int dash_amount;
    public float dash_speed;
    public float dash_sleep_time;
    [Space(5)]

    public float dash_attack_time;
    [Space(5)]

    public float dash_end_time;
    public Vector2 dash_end_speed;
    [Range(0f, 1f)] public float dash_end_run_lerp;
    [Space(5)]

    public float dash_refill_time;

    [Header("어시스트")]
    [Range(0.01f, 0.5f)] public float coyote_time;
    [Range(0.01f, 0.5f)] public float jump_input_buffer_time;
    [Range(0.01f, 0.5f)] public float dash_input_buffer_time;
    [Range(0.01f, 0.5f)] public float grab_input_buffer_time;


    private void OnValidate()
    {
        gravity_strength = -(2 * jump_height) / (jump_time_to_apex * jump_time_to_apex);

        gravity_scale = gravity_strength / Physics2D.gravity.y;

        run_accel_amount = (50 * run_acceleration) / run_max_speed;
        run_deccel_amount = (50 * run_decceleration) / run_max_speed;

        jump_force = Mathf.Abs(gravity_strength) * jump_time_to_apex;

        run_acceleration = Mathf.Clamp(run_acceleration, 0.01f, run_max_speed);
        run_decceleration = Mathf.Clamp(run_decceleration, 0.01f, run_max_speed);
    }
}

