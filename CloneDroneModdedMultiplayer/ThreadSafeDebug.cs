using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLibrary;
using CloneDroneModdedMultiplayer.LowLevelNetworking;
using UnityEngine;

namespace CloneDroneModdedMultiplayer
{
	public static class ThreadSafeDebug
	{
		public static void Log(string msg)
		{
			NetworkingCore.ScheduleForMainThread(() => debug.Log(msg));
		}
		public static void Log(string msg, Color color)
		{
			NetworkingCore.ScheduleForMainThread(() => debug.Log(msg, color));
		}
		public static void Log(object msg)
		{
			NetworkingCore.ScheduleForMainThread(() => debug.Log(msg));
		}
		public static void Log(object msg, Color color)
		{
			NetworkingCore.ScheduleForMainThread(() => debug.Log(msg, color));
		}

		public static void DrawLine(Vector3 point1, Vector3 point2, Color color, float timeToSay = 0)
		{
			NetworkingCore.ScheduleForMainThread(() => debug.DrawLine(point1, point2, color, timeToSay));
		}
		public static void DrawRay(Vector3 point1, Vector3 direction, Color color, float timeToSay = 0)
		{
			NetworkingCore.ScheduleForMainThread(() => debug.DrawRay(point1, direction, color, timeToSay));
		}

	}
}
