using UnityEngine;

public class Troll : Alien
{
    protected override void Update()
    {
        Soldier target;
        if (!_attacking && (target = HasTargetInRange()) != null)
        {
            if (Time.time - _lastAttack >= AttackSpeed)
            {
                Attack(target);
                _lastAttack = Time.time;
            }
            return;
        }

        base.Update();
    }

    private Soldier HasTargetInRange()
    {
        foreach (var soldier in Soldier.All)
        {
            if (Vector3.Distance(soldier.transform.position, transform.position) <= Range)
                return soldier;
        }
        return null;
    }
}