public interface IDamageable
{
    // 피격 시스템 인터페이스 분리
    void TakeDamage(float damage, DamageType type);
}

public enum DamageType
{
    Normal,     // 일반 장애물: 가시, 레이저 등
    Water,      // 물 (지속 데미지) 
    Magnetic    // 자기장 (영혼 상태도 피해를 입음)
}