using System.Collections.Generic;

// ������� [System.Serializable] ����������.
// �� ������� Unity, ��� ������� ����� ������ ����� ���������� � JSON � �������.
[System.Serializable]
public class SaveData
{
    // ������ ��� ������� ��������� ������
    public int currency;

    // ������ ���������� ID ��������� �������.
    // �� ������ �� ���� �����, � ������ ��� ID, ��� ����� ����������.
    public List<string> unlockedPassiveIDs;

    // ����������� �� ���������. ������� "������" ���������� ��� ������ ������.
    public SaveData()
    {
        currency = 0;
        unlockedPassiveIDs = new List<string>();
    }
}