using System.Collections;

public interface IEnemy { Bee Bee { get; } }
public interface IEnemyIdleHandler : IEnemy { IEnumerator IdleRoutine(); }
public interface IEnemyAttackHandler: IEnemy { IEnumerator AttackRoutine(); }
public interface IEnemyMovementHandler : IEnemy { IEnumerator MoveRoutine(); }
public interface IOnButtonPressedHandler { void Toggle(); }