public enum StatType
{
    // --- ����� ��������� ---
    MaxHealth,          // ������������ ��������
    Armor,              // ����� (��������, ��� �������� �����)
    MoveSpeed,          // �������� ������������
    PickupRadius,       // ������ ������� �����/���������

    // --- ���������� ����� ������ ---
    Damage,             // % ������ �����
    AreaOfEffect,       // % ������� �������� (������)
    Cooldown,           // % �������� �����������
    Duration,           // % ������������ ������ (��������, ���� ��� ������� �� ������)
    Amount,             // + � ���������� ��������/�������� (������� �����)

    // --- ����� �������� ---
    ProjectileSpeed,    // % �������� ������ ��������
    RicochetChance,     // ���� �������� (��������, 5 ��� 5%)
    RicochetCount,     // ������������ ���������� �������� (������� ��������)
    /* ProjectilePierce,   // + � ���������� ������ (������� �����)*/ // � ����� �������� �� ������������, �������� ����� ����� ��� �������� ������� �������� ��������


    // --- ����������/����������� ---
    Luck,               // ����� (����� ������ �� ���� �����, ��������� ������)
    ExperienceGain,     // % ��������� �����
    CurrencyGain        // % ��������� ������ ��� ����-����������
}