using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchableEnumAttributeSample : MonoBehaviour
{
	public enum ActionType
	{
		ATACK_01,
		ATACK_02,
		ATACK_03,
		ATACK_04,
		ATACK_AIR_01,
		ATACK_AIR_02,
		ATACK_AIR_03,
		ATACK_AIR_04,
		SPECIAL_01,
		SPECIAL_02,
		SPECIAL_03,
		SPECIAL_04,
		SPECIAL_AIR_01,
		SPECIAL_AIR_02,
		SPECIAL_AIR_03,
		SPECIAL_AIR_04,
		DEFENSE_01,
		DEFENSE_02,
		DEFENSE_03,
		DEFENSE_04,
		DEFENSE_AIR_01,
		DEFENSE_AIR_02,
		DEFENSE_AIR_03,
		DEFENSE_AIR_04,
		HEAL_01,
		HEAL_02,
		HEAL_03,
		HEAL_04,
		UTILITY_01,
		UTILITY_02,
		UTILITY_03,
		UTILITY_04,
		BUFF_01,
		BUFF_02,
		BUFF_03,
		BUFF_04,
		DEBUFF_01,
		DEBUFF_02,
		DEBUFF_03,
		DEBUFF_04,
	}

	[SearchableEnum]
	public ActionType actionType;
}
