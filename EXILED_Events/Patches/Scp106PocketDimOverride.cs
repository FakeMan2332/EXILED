﻿using CustomPlayerEffects;
using Harmony;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EXILED.Patches
{
    [HarmonyPatch(typeof(Scp106PlayerScript), "CallCmdMovePlayer")]
    public class Scp106PocketDimOverride
    {
        public static void Prefix(Scp106PlayerScript __instance, GameObject ply, int t)
        {
			if (!__instance._iawRateLimit.CanExecute(true))
				return;
			if (ply == null)
				return;

			CharacterClassManager component = ply.GetComponent<CharacterClassManager>();
			if (component == null)
				return;
			if (!ServerTime.CheckSynchronization(t) || !__instance.iAm106 || Vector3.Distance(__instance.GetComponent<PlyMovementSync>().RealModelPosition, ply.transform.position) >= 3f || !component.IsHuman())
				return;

			CharacterClassManager component2 = ply.GetComponent<CharacterClassManager>();
			if (component2.GodMode)
				return;
			if (component2.Classes.SafeGet(component2.CurClass).team == Team.SCP)
				return;

			__instance.GetComponent<CharacterClassManager>().RpcPlaceBlood(ply.transform.position, 1, 2f);
			if (Scp106PlayerScript.blastDoor.isClosed)
			{
				bool AllowDamage = true;

				Events.InvokePocketDimDamage(ply, ref AllowDamage);

				if (!AllowDamage)
					return;
				__instance.GetComponent<CharacterClassManager>().RpcPlaceBlood(ply.transform.position, 1, 2f);
				__instance.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(500f, __instance.GetComponent<NicknameSync>().MyNick + " (" + __instance.GetComponent<CharacterClassManager>().UserId + ")", DamageTypes.Scp106, __instance.GetComponent<QueryProcessor>().PlayerId), ply);
			}
			else
			{
				CharacterClassManager component3 = ply.GetComponent<CharacterClassManager>();
				// 079 shit
				foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
				{
					Scp079Interactable.ZoneAndRoom otherRoom = ply.GetComponent<Scp079PlayerScript>().GetOtherRoom();
					Scp079Interactable.InteractableType[] filter = new Scp079Interactable.InteractableType[]
					{
					Scp079Interactable.InteractableType.Door,
					Scp079Interactable.InteractableType.Light,
					Scp079Interactable.InteractableType.Lockdown,
					Scp079Interactable.InteractableType.Tesla,
					Scp079Interactable.InteractableType.ElevatorUse
					};
					bool flag = false;
					foreach (Scp079Interaction scp079Interaction in scp079PlayerScript.ReturnRecentHistory(12f, filter))
					{
						foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interaction.interactable.currentZonesAndRooms)
						{
							if (zoneAndRoom.currentZone == otherRoom.currentZone && zoneAndRoom.currentRoom == otherRoom.currentRoom)
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						scp079PlayerScript.RpcGainExp(ExpGainType.PocketAssist, component3.CurClass);
					}
				}

				// Invoke enter

				bool AllowEnter = true;

				Events.InvokePocketDimEnter(ply, ref AllowEnter);

				if (!AllowEnter)
					return;

				ply.GetComponent<PlyMovementSync>().OverridePosition(Vector3.down * 1998.5f, 0f, true);

				// Invoke damage.

				bool AllowDamage = true;


				Events.InvokePocketDimDamage(ply, ref AllowDamage);

				if (!AllowDamage)
					return;
				__instance.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(40f, __instance.GetComponent<NicknameSync>().MyNick + " (" + __instance.GetComponent<CharacterClassManager>().UserId + ")", DamageTypes.Scp106, __instance.GetComponent<QueryProcessor>().PlayerId), ply);

			}
			PlayerEffectsController componentInParent = ply.GetComponentInParent<PlayerEffectsController>();
			componentInParent.GetEffect<Corroding>("Corroding").isInPd = true;
			componentInParent.EnableEffect("Corroding");
		}
    }
}
