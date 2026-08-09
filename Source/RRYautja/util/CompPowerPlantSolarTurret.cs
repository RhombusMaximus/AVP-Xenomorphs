using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
    // Token: 0x02000246 RID: 582
    public class CompProperties_SolarCell : CompProperties_Power
    {
        // Token: 0x06000A9E RID: 2718 RVA: 0x00055040 File Offset: 0x00053440
        public CompProperties_SolarCell()
        {
            this.compClass = typeof(CompPowerPlantSolarCell);
        }
        public float solarEnergyMax = 170f;
        // Token: 0x04000477 RID: 1143
        public float storedEnergyMax = 1000f;

        // Token: 0x04000478 RID: 1144
        public float efficiency = 0.5f;
    }

    // Token: 0x0200042F RID: 1071
    [StaticConstructorOnStartup]
    public class CompPowerPlantSolarCell : CompPowerPlant
    {

        public CompPowerPlantSolarCell()
        {

        }

        public override void SetUpPowerVars()
        {
            base.SetUpPowerVars();
            CompProperties_SolarCell props = this.Props;
            this.PowerOutput = -1f * props.basePowerConsumption;
            this.powerLastOutputted = (props.basePowerConsumption <= 0f);
        }

        // Token: 0x1700028D RID: 653 transNet
        // (get) Token: 0x060012AF RID: 4783 RVA: 0x0008FC01 File Offset: 0x0008E001
        protected override float DesiredPowerOutput
        {
            get
            {
                return Mathf.Lerp(0f, this.Props.solarEnergyMax, this.parent.Map.skyManager.CurSkyGlow) * this.RoofedPowerOutputFactor;
            }
        }

        // Token: 0x1700028E RID: 654
        // (get) Token: 0x060012B0 RID: 4784 RVA: 0x0008FC30 File Offset: 0x0008E030
        private float RoofedPowerOutputFactor
        {
            get
            {
                int num = 0;
                int num2 = 0;
                for (int i = 0; i < GenAdj.AdjacentCellsAndInside.Length; i++)
                {
                    IntVec3 c = this.parent.Position + GenAdj.AdjacentCellsAndInside[i];
                    num++;
                    if (this.parent.Map.roofGrid.Roofed(c))
                    {
                        num2++;
                    }
                }
                return (float)(num - num2) / (float)num;
            }
        }

        // Token: 0x1700027E RID: 638
        // (get) Token: 0x06001273 RID: 4723 RVA: 0x0008E8BC File Offset: 0x0008CCBC
        public float AmountCanAccept
        {
            get
            {
                if (this.parent.IsBrokenDown())
                {
                    return 0f;
                }
                CompProperties_SolarCell props = this.Props;
                return (props.storedEnergyMax - this.storedEnergy) / props.efficiency;
            }
        }

        // Token: 0x1700027F RID: 639
        // (get) Token: 0x06001274 RID: 4724 RVA: 0x0008E8FA File Offset: 0x0008CCFA
        public float StoredEnergy
        {
            get
            {
                return this.storedEnergy;
            }
        }

        // Token: 0x17000280 RID: 640
        // (get) Token: 0x06001275 RID: 4725 RVA: 0x0008E902 File Offset: 0x0008CD02
        public float StoredEnergyPct
        {
            get
            {
                return this.storedEnergy / this.Props.storedEnergyMax;
            }
        }

        // Token: 0x17000281 RID: 641
        // (get) Token: 0x06001276 RID: 4726 RVA: 0x0008E916 File Offset: 0x0008CD16
        public new CompProperties_SolarCell Props
        {
            get
            {
                return (CompProperties_SolarCell)this.props;
            }
        }

        // Token: 0x06001277 RID: 4727 RVA: 0x0008E924 File Offset: 0x0008CD24
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.storedEnergy, "storedPower", 0f, false);
            CompProperties_SolarCell props = this.Props;
            if (this.storedEnergy > props.storedEnergyMax)
            {
                this.storedEnergy = props.storedEnergyMax;
            }
        }

        // Token: 0x06001278 RID: 4728 RVA: 0x0008E971 File Offset: 0x0008CD71
        public override void CompTick()
        {
            base.CompTick();
            this.AddEnergy(this.DesiredPowerOutput * CompPower.WattsToWattDaysPerTick);
            if (this.transNet==null || (this.transNet != null && this.transNet.CurrentStoredEnergy() > 0))
            {
                this.DrawPower(Mathf.Min(this.EnergyOutputPerTick, this.storedEnergy));
            }
            if (this.storedEnergy == 0)
            {
                this.PowerOn = false;
            }
            else
            {
                this.PowerOn = true;
            }
        }

        // Token: 0x06001279 RID: 4729 RVA: 0x0008E998 File Offset: 0x0008CD98
        public void AddEnergy(float amount)
        {
            if (amount < 0f)
            {
                Log.Error("Cannot add negative energy " + amount);
                return;
            }
            if (amount > this.AmountCanAccept)
            {
                amount = this.AmountCanAccept;
            }
            amount *= this.Props.efficiency;
            bool selected = Find.Selector.SelectedObjects.Contains(this.parent) && Prefs.DevMode && DebugSettings.godMode;
            if (selected) Log.Message(string.Format("gain: {0}", amount));
            this.storedEnergy += amount;
        }

        // Token: 0x0600127A RID: 4730 RVA: 0x0008E9F8 File Offset: 0x0008CDF8
        public void DrawPower(float amount)
        {
            this.storedEnergy -= amount;
            bool selected = Find.Selector.SelectedObjects.Contains(this.parent) && Prefs.DevMode && DebugSettings.godMode;
            if (selected) Log.Message(string.Format("loss: {0}", amount));
            if (this.storedEnergy < 0f)
            {
                Log.Error("Drawing power we don't have from " + this.parent);
                this.storedEnergy = 0f;
            }
        }

        // Token: 0x0600127B RID: 4731 RVA: 0x0008EA44 File Offset: 0x0008CE44
        public void SetStoredEnergyPct(float pct)
        {
            pct = Mathf.Clamp01(pct);
            this.storedEnergy = this.Props.storedEnergyMax * pct;
        }

        // Token: 0x0600127C RID: 4732 RVA: 0x0008EA61 File Offset: 0x0008CE61
        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "Breakdown")
            {
                this.DrawPower(this.StoredEnergy);
            }
            base.ReceiveCompSignal(signal);
        }

        // Token: 0x0600127D RID: 4733 RVA: 0x0008EA80 File Offset: 0x0008CE80
        public override string CompInspectStringExtra()
        {
            CompProperties_SolarCell props = this.Props;
            string text = string.Concat(new string[]
            {
                "PowerBatteryStored".Translate(),
                ": ",
                this.storedEnergy.ToString("F0"),
                " / ",
                props.storedEnergyMax.ToString("F0"),
                " Wd"
            });
            string text2 = text;
            text = string.Concat(new string[]
            {
                text2,
                "\n",
                "PowerBatteryEfficiency".Translate(),
                ": ",
                (props.efficiency * 100f).ToString("F0"),
                "%"
            });
            text += "\n" + "PowerOutput".Translate() + ": " + (this.PowerOutput).ToString() + " W";
            /*
            text += "\n" + "PowerNeeded".Translate() + ": " + (-this.PowerOutput).ToString() + " W";
            if (this.PowerNet == null)
            {
                text += "\n" + "PowerNotConnected".Translate();
            }
            else
            {
                string value = (this.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick).ToString("F0");
                string value2 = this.PowerNet.CurrentStoredEnergy().ToString("F0");
                text += "\n" + "PowerConnectedRateStored".Translate(value, value2);
            }
            */
            return text;
        }

        // Token: 0x0600127E RID: 4734 RVA: 0x0008EBB0 File Offset: 0x0008CFB0
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo c in base.CompGetGizmosExtra())
            {
                yield return c;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Fill",
                    action = delegate ()
                    {
                        this.SetStoredEnergyPct(1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = delegate ()
                    {
                        this.SetStoredEnergyPct(0f);
                    }
                };
            }
            yield break;
        }

        // Token: 0x060012B1 RID: 4785 RVA: 0x0008FCBC File Offset: 0x0008E0BC
        public override void PostDraw()
        {
            base.PostDraw();
            GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
            r.center = this.parent.DrawPos + Vector3.up * 0.1f;
            r.center.x += 0.45f;
            r.size = CompPowerPlantSolarCell.BarSize;
            r.fillPercent = this.DesiredPowerOutput / this.Props.solarEnergyMax;
            r.filledMat = CompPowerPlantSolarCell.PowerPlantSolarBarFilledMat;
            r.unfilledMat = CompPowerPlantSolarCell.PowerPlantSolarBarUnfilledMat;
            r.margin = 0.07f;
            Rot4 rotation = this.parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);

            GenDraw.FillableBarRequest r2 = default(GenDraw.FillableBarRequest);
            r2.center = this.parent.DrawPos + Vector3.up * 0.1f;
            r2.center.x -= 0.45f;
            r2.size = CompPowerPlantSolarCell.BarSize;
            r2.fillPercent = StoredEnergyPct;
            r2.filledMat = CompPowerPlantSolarCell.BatterySolarBarFilledMat;
            r2.unfilledMat = CompPowerPlantSolarCell.BatterySolarBarUnfilledMat;
            r2.margin = 0.07f;
            Rot4 rotation2 = this.parent.Rotation;
            rotation2.Rotate(RotationDirection.Clockwise);
            r2.rotation = rotation2;
            GenDraw.DrawFillableBar(r2);
        }

        private bool powerLastOutputted;

    //    public float powerOutputInt;
        // Token: 0x04000B57 RID: 2903
        private float storedEnergy;

        // Token: 0x04000B58 RID: 2904
        private const float SelfDischargingWatts = 5f;

        // Token: 0x04000B6A RID: 2922
        private const float FullSunPower = 1700f;

        // Token: 0x04000B6B RID: 2923
        private const float NightPower = 0f;

        // Token: 0x04000B6C RID: 2924
        private static readonly Vector2 BarSize = new Vector2(0.5f, 0.03f);

        // Token: 0x04000B6D RID: 2925
        private static readonly Material PowerPlantSolarBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f), false);

        // Token: 0x04000B6E RID: 2926
        private static readonly Material PowerPlantSolarBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f), false);

        // Token: 0x04000B6D RID: 2925
        private static readonly Material BatterySolarBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.275f, 0.1f), false);

        // Token: 0x04000B6E RID: 2926
        private static readonly Material BatterySolarBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f), false);
    }
}
