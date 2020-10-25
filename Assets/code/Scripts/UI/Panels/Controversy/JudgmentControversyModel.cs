﻿using System;
using System.Collections.Generic;
using Config.Data;
using UI.Panels.Providers.DataProviders;
using UnityEngine;

namespace UI.Panels
{
    public enum EnumControversyStage
    {
        Begin,
        Entrance,
        StageOne,
        StageOneLose,
        StageOneWin,
        StageTwo,
        Win,
        MissSpecial,
        Wrong,
    }
    public class BarrageItem
    {
        public int ID;
        public List<BarrageTextItem> Items;
        public bool IsSpecial;
        public float BornTime;
        public int CorrectIndex;
    }

    public class BarrageTextItem
    {
        public int ID;
        public string Text;
        public bool IsHighLight;
        public int Index;
        public BarrageItem BarrageInfo;
    }
    public class JudgmentControversyModel: UIModel
    {
        #region template method
        public override void Initialize(IUiPanel uiPanel )
        {
            base.Initialize(uiPanel);
            var config = CommonConfig.Data;
            m_panel = (JudgmentControversyPanel) uiPanel;
            m_stageOneClearPoint = config.ControversyStageOnePoint;
            m_stageTwoClearPoint = config.ControversyStageTwoPoint1;
        }

        public override void DeInitialize()
        {
            base.DeInitialize();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void ShowData(DataProvider data)
        {
            base.ShowData(data);
        }

        public override void UpdateData(DataProvider data)
        {
            base.UpdateData(data);
        }

        public override void Tick()
        {
            base.Tick();
            if (IsBeginBarrage)
            {
                return;
            }

            if (IsHeavyAttack)
            {
                IsHeavyAttack = false;
                m_heavyAttackColdTime = m_heavyAttackInterval;
                CheckCharge();
            }

            if (IsNormalAttack)
            {
                IsNormalAttack = false;
                m_normalAttackColdTime = m_normalAttackInterval;
            }
            
            if (Input.GetKeyDown(KeyCode.A) && IsCharging)
            {
                IsHeavyAttack = true;
            }

            if (Input.GetKeyDown(KeyCode.S) && m_normalAttackColdTime <= 0)
            {
                IsNormalAttack = true;
            }

            if (m_heavyAttackColdTime > 0)
            {
                m_heavyAttackColdTime -= Time.deltaTime;
                if (m_heavyAttackColdTime <= 0)
                {
                    CheckCharge();
                }
            }

            if (m_normalAttackColdTime > 0)
            {
                m_normalAttackColdTime -= Time.deltaTime;
            }
            
            if (CurStage == EnumControversyStage.StageOne || CurStage == EnumControversyStage.StageTwo)
            {
                for (int i = 0; i < NormalBarrageInfos.Count; i++)
                {
                    if (NormalBarrageInfos[i].BornTime <= m_time)
                    {
                        m_panel.SendBarrage(NormalBarrageInfos[i]);
                        NormalBarrageInfos.RemoveAt(i);
                        i--;
                    }
                }
            }

            m_time += Time.deltaTime;
        }

        public override void LateTick()
        {
            base.LateTick();
        }

        public override void SubpanelChanged(UIPanelType type, DataProvider data = null)
        {
            base.SubpanelChanged(type,data);
        }

        public override void SubpanelDataChanged(UIPanelType type, DataProvider data)
        {
            base.SubpanelDataChanged(type,data);
        }
        #endregion
        
        public void ChangeStage(EnumControversyStage stage)
        {
            CurStage = stage;
            switch (CurStage)
            {
                case EnumControversyStage.StageOne:
                    InitStageOneData();
                    break;
                case EnumControversyStage.StageTwo:
                    InitStageTwoData();
                    break;
            }
            
            m_panel.ChangeStage();
        }
        
        public bool IsStageClear()
        {
            var point = m_slashedBarrageAmount * 100 / TotalBarrageAmount;

            switch (CurStage)
            {
                case EnumControversyStage.StageOne:
                    return point >= m_stageOneClearPoint;
                case EnumControversyStage.StageTwo:
                    return point >= m_stageTwoClearPoint;
                default:
                    return true;
            }
        }
        
        public bool SlashBarrage(BarrageTextItem barrage)
        {
            if (barrage.BarrageInfo == SpecialBarrageInfo)
            {
                if (!IsHeavyAttack)
                {
                    return false;
                }

                if (barrage.Index == barrage.BarrageInfo.CorrectIndex)
                {
                    ChangeStage(EnumControversyStage.Win);
                    return true;
                }
                else
                {
                    SlashedSpecialIndex = barrage.Index;
                    ChangeStage(EnumControversyStage.Wrong);
                    return false;
                }
            }
            if (IsHeavyAttack)
            {
                SlashBarrageWithoutCheck(barrage);
                return true;
            }

            if (IsSlashed(barrage.ID))
            {
                return false;
            }
            if ( barrage.Index == barrage.BarrageInfo.CorrectIndex)
            {
                SlashBarrage(barrage.ID);
                return true;
            }
            return false;
        }

        private void SlashBarrageWithoutCheck(BarrageTextItem barrage)
        {
            if (IsSlashed(barrage.ID))
            {
                return;
            }

            SlashBarrage(barrage.ID);
        }

        private void SlashBarrage(int id)
        {
            m_heavyAttackColdTime -= m_heavyAttackRecover;
            m_slashedBarrageAmount++;
            CheckCharge();
            m_slashedBarrageLst.Add(id);
        }

        public bool BarragePassed(BarrageItem barrage)
        {
            if (IsSlashed(barrage.ID))
            {
                return false;
            }
            m_slashedBarrageLst.Add(barrage.ID);
            if (barrage.IsSpecial)
            {
                ChangeStage(EnumControversyStage.MissSpecial);
            }
            else
            {
                NormalBarrageInfos.Remove(barrage);
                if (NormalBarrageInfos.Count == 0)
                {
                    CheckStage();
                }
            }

            return true;
        }
        
        public bool IsSlashed(int barrageID)
        {
            return m_slashedBarrageLst.Contains(barrageID);
        }

        private void CheckCharge()
        {
            IsCharging = m_heavyAttackColdTime <= 0;
            m_panel.CheckCharge();
        }

        public void Clear()
        {
            TotalBarrageAmount = 0;
            m_time = 0;
        }

        private void SetInfo(string controversyID)
        {
            m_controversyID = controversyID;
            ControversyConfig = ControversyConfig.GetConfigByKey(controversyID);
            EnemyConfig = ControversyCharacterConfig.GetConfigByKey(ControversyConfig.leftCharacterID);

            var specialBarrageID = ControversyConfig.specialBarrageID;
            SpecialBarrageInfo = GetSpecialBarrageItem(specialBarrageID);
            SpecialBarrageConfig = ControversySpecialBarrageConfig.GetConfigByKey(SpecialBarrageInfo.ID);
            ChangeStage(EnumControversyStage.Entrance);
        }

        private void CheckStage()
        {
            switch (CurStage)
            {
                case EnumControversyStage.Begin:
                    ChangeStage(EnumControversyStage.StageOne);
                    break;
                case EnumControversyStage.StageOne:
                    if (IsStageClear())
                    {
                        ChangeStage(EnumControversyStage.StageOneWin);
                    }
                    else
                    {
                        ChangeStage(EnumControversyStage.StageOneLose);
                    }
                    break;
                case EnumControversyStage.StageOneLose:
                    ChangeStage(EnumControversyStage.StageOne);
                    break;
                case EnumControversyStage.StageOneWin:
                    ChangeStage(EnumControversyStage.StageTwo);
                    break;
                case EnumControversyStage.StageTwo:
                    if (SlashedSpecialIndex != 0)
                    {
                        ChangeStage(EnumControversyStage.Wrong);
                    }
                    else
                    {
                        ChangeStage(EnumControversyStage.MissSpecial);
                    }
                    break;
                case EnumControversyStage.MissSpecial:
                case EnumControversyStage.Wrong:
                    ChangeStage(EnumControversyStage.StageTwo);
                    break;
            }
        }

        private void InitStageOneData()
        {
            NormalBarrageInfos.Clear();

            var barrageInfos = GameRunTimeData.Instance.ControllerManager.ControversyController.GetBarrageInfoByGroup(ControversyConfig
                .stageOneBarrageGroupID);
            foreach (var barrageInfo in barrageInfos)
            {
                NormalBarrageInfos.Add(GetNormalBarrageItem(barrageInfo));
            }

            TotalBarrageAmount = NormalBarrageInfos.Count;
            m_slashedBarrageAmount = 0;
            m_time = 0;
        }

        private void InitStageTwoData()
        {
            NormalBarrageInfos.Clear();

            var barrageInfos = GameRunTimeData.Instance.ControllerManager.ControversyController.GetBarrageInfoByGroup(ControversyConfig
                .stageTwoBarrageGroupID);
            foreach (var barrageInfo in barrageInfos)
            {
                NormalBarrageInfos.Add(GetNormalBarrageItem(barrageInfo));
            }
            TotalBarrageAmount = NormalBarrageInfos.Count;
            m_slashedBarrageAmount = 0;
            m_time = 0;
        }

        private BarrageItem GetSpecialBarrageItem(int specialBarrageID)
        {
            BarrageItem item = new BarrageItem(){ID = specialBarrageID,IsSpecial = true};
            var specialBarrageConfig = ControversySpecialBarrageConfig.GetConfigByKey(specialBarrageID);
            item.Items = ProcessBarrageText(specialBarrageID, specialBarrageConfig.text);
            item.CorrectIndex = specialBarrageConfig.correctIndex;
            item.BornTime = specialBarrageConfig.bornTime;
            return item;
        }

        private BarrageItem GetNormalBarrageItem(ControversyBarrageConfig barrageConfig)
        {
            BarrageItem item = new BarrageItem(){ID = barrageConfig.ID,IsSpecial = false};

            item.Items = ProcessBarrageText(barrageConfig.ID, barrageConfig.text);
            item.CorrectIndex = barrageConfig.correctIndex;
            item.BornTime = barrageConfig.bornTime;
            return item;
        }
        
        private List<BarrageTextItem> ProcessBarrageText(int barrageID, string text)
        {
            List<BarrageTextItem> items = new List<BarrageTextItem>();
            bool isBeginWithHighlight = text.StartsWith("HighLight_{");
            string[] strs = text.Split(new string[] {"HighLight_{"},StringSplitOptions.RemoveEmptyEntries);
            int index = 1;
            for (int i = 0; i < strs.Length; i++)
            {
                if (i == 0 && !isBeginWithHighlight)
                {
                    items.Add(new BarrageTextItem(){ID = barrageID,Index = 0,IsHighLight = false, Text = strs[i]});
                    continue;
                }

                var normalStr = strs[i].Substring(strs[i].IndexOf('}')+1);
                if (normalStr.Length >= 0)
                {
                    items.Add(new BarrageTextItem() {ID = barrageID, Index = 0, IsHighLight = false, Text = normalStr});
                }

                var specialStr = strs[i].Substring(0,strs[i].IndexOf('}'));
                items.Add(new BarrageTextItem(){ID = barrageID,Index = index++,IsHighLight = true, Text = specialStr});
            }

            return items;
        }
        
        #region Member
        public List<BarrageItem> NormalBarrageInfos { get; private set; } = new List<BarrageItem>();
        public BarrageItem SpecialBarrageInfo;
        public ControversySpecialBarrageConfig SpecialBarrageConfig;
        public ControversyConfig ControversyConfig;
        public ControversyCharacterConfig EnemyConfig;
        public bool IsBeginBarrage;
        public int TotalBarrageAmount;


        public EnumControversyStage CurStage;
        public bool IsCharging { get; private set; }
        public bool IsHeavyAttack;
        public bool IsNormalAttack;
        public int SlashedSpecialIndex;


        private JudgmentControversyPanel m_panel;
        private string m_controversyID;
        private int m_slashedBarrageAmount;
        private int m_stageOneClearPoint;
        private int m_stageTwoClearPoint;
        private List<int> m_slashedBarrageLst = new List<int>();
        private List<BarrageItem> SentBarrageInfos = new List<BarrageItem>();
        private float m_time=0;
        private float m_normalAttackInterval;
        private float m_heavyAttackInterval;
        private float m_heavyAttackRecover;

        private float m_normalAttackColdTime;
        private float m_heavyAttackColdTime;

        #endregion
    }
}