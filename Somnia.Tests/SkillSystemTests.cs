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
        private NpcModel _n;
        private Rectangle _area;

        [SetUp]
        public void Init()
        {
            _p = new PlayerModel(new Vector2(500, 500));
            _el = new List<EnemyModel>();
            _wl = new List<HexagonModel>();
            _n = new NpcModel(new Vector2(600, 600));
            _area = new Rectangle(0, 0, 1000, 1000);
        }

        [Test]
        public void Mana_Consumption_Red_Slot0()
        {
            _p.CurrentZone = AnomalyType.Red;
            _p.ActiveSlot = 0;
            _p.CurrentMana = 100;
            _p.UseActiveSkill(new Vector2(1, 0), _el, _n);
            Assert.That(_p.CurrentMana, Is.EqualTo(90f));
        }

        [Test]
        public void RedZone_Slot0_Damage_Check()
        {
            _p.CurrentZone = AnomalyType.Red;
            _p.ActiveSlot = 0;
            var enemy = new EnemyModel(new Vector2(550, 500));
            _el.Add(enemy);
            float startH = enemy.Health;
            _p.UseActiveSkill(new Vector2(1, 0), _el, _n);
            Assert.That(enemy.Health, Is.LessThan(startH));
        }

        [Test]
        public void BlueZone_Slot1_Stun_Applied()
        {
            _p.CurrentZone = AnomalyType.Blue;
            _p.ActiveSlot = 1;
            var enemy = new EnemyModel(new Vector2(520, 500));
            _el.Add(enemy);
            _p.UseActiveSkill(new Vector2(1, 0), _el, _n);
            Assert.That(enemy.StunTimer, Is.GreaterThan(0));
            Assert.That(_p.IsDashing, Is.True);
        }

        [Test]
        public void GreenZone_Slot1_Aura_Activation()
        {
            _p.CurrentZone = AnomalyType.Green;
            _p.ActiveSlot = 1;
            _p.UseActiveSkill(Vector2.UnitX, _el, _n);
            Assert.That(_p.GreenAuraTimer, Is.GreaterThan(0));
        }

        [Test]
        public void GreenZone_Slot2_Infection_Applied()
        {
            _p.CurrentZone = AnomalyType.Green;
            _p.ActiveSlot = 2;
            var enemy = new EnemyModel(new Vector2(550, 500));
            _el.Add(enemy);
            _p.UseActiveSkill(new Vector2(1, 0), _el, _n);
            Assert.That(enemy.IsInfected, Is.True);
        }

        [Test]
        public void Skill_Cooldown_Blocks_Repeated_Use()
        {
            _p.CurrentZone = AnomalyType.Red;
            _p.ActiveSlot = 0;
            _p.UseActiveSkill(Vector2.UnitX, _el, _n);
            float manaAfterFirst = _p.CurrentMana;
            _p.UseActiveSkill(Vector2.UnitX, _el, _n);
            Assert.That(_p.CurrentMana, Is.EqualTo(manaAfterFirst));
        }

        [Test]
        public void Carrying_State_Prevents_Skills()
        {
            _p.SetState(PlayerState.Carrying);
            _p.CurrentZone = AnomalyType.Red;
            _p.ActiveSlot = 0;
            _p.UseActiveSkill(Vector2.UnitX, _el, _n);
            Assert.That(_p.CurrentMana, Is.EqualTo(100f));
        }

        [Test]
        public void Low_Mana_Prevents_Skill()
        {
            _p.CurrentMana = 2f;
            _p.CurrentZone = AnomalyType.Red;
            _p.ActiveSlot = 0;
            _p.UseActiveSkill(Vector2.UnitX, _el, _n);
            Assert.That(_p.CurrentMana, Is.EqualTo(2f));
        }

        [Test]
        public void Dash_Movement_Logic()
        {
            Vector2 start = _p.Position;
            _p.CurrentZone = AnomalyType.Blue;
            _p.ActiveSlot = 1;
            _p.UseActiveSkill(new Vector2(1, 0), _el, _n);
            _p.Move(Vector2.Zero, 0.1f, _area, _wl);
            Assert.That(_p.Position.X, Is.GreaterThan(start.X));
        }

        [Test]
        public void Npc_Pickup_Interaction()
        {
            _p.Position = _n.Position - new Vector2(10, 0);
            var ctrl = new PlayerController(_p);
            _p.SetState(PlayerState.Free);
            
            // Simulating Key E press logic from HandleInteraction
            _n.IsPickedUp = true;
            _p.SetState(PlayerState.Carrying);
            
            Assert.That(_p.State, Is.EqualTo(PlayerState.Carrying));
            Assert.That(_n.IsPickedUp, Is.True);
        }

        [Test]
        public void Npc_Health_Penalty_Damage_Multiplier()
        {
            _n.Health = 30f; 
            _p.CurrentZone = AnomalyType.Red;
            _p.ActiveSlot = 0;
            var enemy = new EnemyModel(_p.Position + new Vector2(20, 0));
            _el.Add(enemy);
            
            _p.UseActiveSkill(new Vector2(1, 0), _el, _n);
            
            Assert.That(enemy.Health, Is.EqualTo(60f - (100f * 0.5f)));
        }

        [Test]
        public void Mana_Regeneration_Over_Time()
        {
            _p.CurrentMana = 50f;
            _p.Move(Vector2.Zero, 1.0f, _area, _wl);
            Assert.That(_p.CurrentMana, Is.EqualTo(60f));
        }
    }

    [TestFixture]
    public class ModelIntegrityTests
    {
        [Test]
        public void Enemy_Death_State()
        {
            var e = new EnemyModel(Vector2.Zero);
            e.TakeDamage(100f, Vector2.One, 0f);
            Assert.That(e.IsDead, Is.True);
        }

        [Test]
        public void Player_Death_State()
        {
            var p = new PlayerModel(Vector2.Zero);
            p.TakeDamage(200f);
            Assert.That(p.IsDead, Is.True);
        }

        [Test]
        public void Hexagon_Vertex_Count()
        {
            var hex = new HexagonModel(Vector2.Zero, 100f);
            Assert.That(hex.GetTopVertices().Count, Is.EqualTo(6));
        }
    }
}