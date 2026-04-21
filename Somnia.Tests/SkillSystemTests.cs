using NUnit.Framework;
using Microsoft.Xna.Framework;
using Somnia.Game.Models;
using Somnia.Game.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace Somnia.Tests
{
    [TestFixture]
    public class SkillSystemTests
    {
        private PlayerModel _p;
        private List<EnemyModel> _el;
        private List<HexagonModel> _wl;

        [SetUp]
        public void Init()
        {
            _p = new PlayerModel(new Vector2(0, 0));
            _el = new List<EnemyModel>();
            _wl = new List<HexagonModel>();
        }

        [Test]
        public void Mana_Consumption_Validation()
        {
            _p.CurrentMana = 50;
            float initial = _p.CurrentMana;
            _p.ActiveSlot = 0;
            _p.ExecuteSkill();
            Assert.Less(_p.CurrentMana, initial);
        }

        [Test]
        public void Skill_Locked_On_Zero_Mana()
        {
            _p.CurrentMana = 0;
            _p.ActiveSlot = 0;
            bool success = _p.CanExecuteActiveSkill();
            Assert.IsFalse(success);
        }

        [Test]
        public void Cooldown_Blocks_Execution()
        {
            _p.Cd1 = 2.0f;
            _p.ActiveSlot = 0;
            Assert.IsFalse(_p.CanExecuteActiveSkill());
        }

        [Test]
        public void RedZone_Ability_Range_Check()
        {
            _p.CurrentZone = AnomalyType.Red;
            _p.ActiveSlot = 1;
            EnemyModel e = new EnemyModel(new Vector2(10, 10), EnemyType.Melee);
            _el.Add(e);
            
            float h = e.Health;
            _p.HandleSkillImpact(_el);
            Assert.Less(e.Health, h);
        }

        [Test]
        public void BlueZone_Slow_Effect_Applied()
        {
            _p.CurrentZone = AnomalyType.Blue;
            EnemyModel e = new EnemyModel(new Vector2(5, 5), EnemyType.Melee);
            _el.Add(e);

            _p.ApplyBlueSkill(e);
            Assert.Greater(e.SlowTimer, 0);
        }

        [Test]
        public void NeutralZone_Skill_Projectile_Spawn()
        {
            _p.CurrentZone = AnomalyType.Neutral;
            _p.ActiveSlot = 0;
            var projectiles = new List<ProjectileModel>();
            _p.FireProjectile(projectiles);
            Assert.AreEqual(1, projectiles.Count);
        }

        [Test]
        public void GreenAura_Healing_NPC()
        {
            NpcModel n = new NpcModel(new Vector2(10, 10));
            n.Health = 50;
            _p.Position = new Vector2(0, 0);
            _p.GreenAuraTimer = 5.0f;

            _p.UpdateAuraImpact(n);
            Assert.Greater(n.Health, 50);
        }

        [Test]
        public void Viral_Effect_Spreads_To_Nearby()
        {
            EnemyModel e1 = new EnemyModel(new Vector2(0, 0), EnemyType.Melee);
            EnemyModel e2 = new EnemyModel(new Vector2(10, 10), EnemyType.Melee);
            e1.IsInfected = true;
            _el.Add(e1);
            _el.Add(e2);

            _el.ElementAt(0).UpdateInfection(_el);
            Assert.IsTrue(_el.ElementAt(1).IsInfected);
        }

        [Test]
        public void Distance_Calculation_LaTeX()
        {
            Vector2 v1 = new Vector2(0, 0);
            Vector2 v2 = new Vector2(3, 4);
            float d = Vector2.Distance(v1, v2);
            float expected = 5f; 
            Assert.AreEqual(expected, d);
        }
    }
}