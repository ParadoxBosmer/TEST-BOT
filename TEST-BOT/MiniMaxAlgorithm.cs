using SharedLibrary;

namespace TEST_BOT;

public class MiniMaxAlgorithm
{
    public Entity? GetBestTarget(Map map, Player player)
    {
        Entity? bestTarget = null;
        int bestScore = int.MinValue;

        var potentialTargets = GetTargetsInRange(map, player);

        foreach (var target in potentialTargets)
        {
            if (target.Health <= 0) continue;
            if (IsAllied(player, target)) continue;

            int score = EvaluateTarget(map, player, target, depth: 2);

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = target;
            }
        }

        return bestTarget;
    }

    public int EvaluateTarget(Map map, Player player, Entity target, int depth = 1)
    {
        if (target.Health <= 0) return int.MinValue;

        int score = 0;

        int damageDealt = Math.Max(1, player.AttackPower);
        int remainingHealth = target.Health - damageDealt;

        if (remainingHealth <= 0)
        {
            score += 1500;
        }
        else
        {
            score += (damageDealt * 100) / Math.Max(1, target.MaxHealth);
            score += ((target.MaxHealth - remainingHealth) * 50) / Math.Max(1, target.MaxHealth);
        }

        if (target is Player targetPlayer)
        {
            int threatScore = targetPlayer.AttackPower * targetPlayer.AttackRange * 20;
            score += threatScore;

            if (targetPlayer.Health < targetPlayer.MaxHealth / 2)
            {
                score += 200;
            }
        }
        else if (target is Entity monsterTarget && monsterTarget.SummonedByPlayerId.HasValue)
        {
            score += (monsterTarget.AttackPower * 30);
        }

        int distanceToTarget = ManhattanDistance(player.Position, target.Position);
        if (distanceToTarget <= player.AttackRange)
        {
            score += (player.AttackRange - distanceToTarget + 1) * 50;
        }

        if (depth > 0)
        {
            int counterDamage = EvaluateBestOpponentResponse(map, player, target, remainingHealth);
            score -= counterDamage;
        }

        return score;
    }

    private int EvaluateBestOpponentResponse(Map map, Entity attacker, Entity selectedTarget, int selectedTargetRemainingHealth)
    {
        if (selectedTargetRemainingHealth > 0 && !IsAllied(attacker, selectedTarget))
        {
            return EvaluateCounterAttack(selectedTarget, attacker);
        }

        int bestResponse = 0;
        foreach (var field in map.Grid)
        {
            var entity = field.Entity;
            if (entity == null || entity.Health <= 0 || IsAllied(attacker, entity))
            {
                continue;
            }

            bestResponse = Math.Max(bestResponse, EvaluateCounterAttack(entity, attacker));
        }

        return bestResponse;
    }
    private int EvaluateCounterAttack(Entity opponent, Entity attacker)
    {
        int distanceToOpponent = ManhattanDistance(attacker.Position, opponent.Position);
        if (distanceToOpponent > opponent.AttackRange)
        {
            return 0;
        }

        int counterScore = opponent.AttackPower * 100;
        if (opponent.Health > opponent.MaxHealth / 2)
        {
            counterScore += 150;
        }

        return counterScore;
    }

    private List<Entity> GetTargetsInRange(Map map, Entity attacker)
    {
        var targets = new List<Entity>();
        var seenIds = new HashSet<int>();

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        for (int dir = 0; dir < dx.Length; dir++)
        {
            for (int i = 1; i <= attacker.AttackRange; i++)
            {
                int x = attacker.Position.X + dx[dir] * i;
                int y = attacker.Position.Y + dy[dir] * i;

                if (x < 0 || x >= map.X || y < 0 || y >= map.Y)
                {
                    break;
                }

                Position position = new(x, y);
                var field = map.Grid.Find(tile => tile.Position != null && tile.Position.Equals(position));
                if (field == null)
                {
                    continue;
                }

                if (field.Entity == null)
                {
                    continue;
                }

                if (field.Entity.Id != attacker.Id && seenIds.Add(field.Entity.Id))
                {
                    targets.Add(field.Entity);
                }

                break;
            }
        }

        return targets;
    }

    private bool IsAllied(Entity attacker, Entity target)
    {
        if (attacker.Id == target.Id)
        {
            return true;
        }

        if (attacker is Player attackerPlayer)
        {
            return target is Player targetPlayer && targetPlayer.Id == attackerPlayer.Id
                   || target.SummonedByPlayerId == attackerPlayer.Id;
        }

        if (attacker.SummonedByPlayerId.HasValue)
        {
            int summonerId = attacker.SummonedByPlayerId.Value;
            return target.SummonedByPlayerId == summonerId
                   || target is Player targetPlayer && targetPlayer.Id == summonerId;
        }

        return false;
    }

    private int ManhattanDistance(Position pos1, Position pos2)
    {
        return Math.Abs(pos1.X - pos2.X) + Math.Abs(pos1.Y - pos2.Y);
    }
}