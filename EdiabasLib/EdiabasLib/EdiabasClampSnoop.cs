#if NETFRAMEWORK
using System;
using System.Collections.Generic;

namespace EdiabasLib
{
    /// <summary>
    /// Transport-agnostic detection of the vehicle clamp (KL15) state from the diagnostic jobs ISTA
    /// already runs. Pure functions, no I/O and no state: an interface's <c>OnJobCompleted</c> hook
    /// passes the job name, its string args and the result sets it just produced, and gets back
    /// <c>true</c> = KL15 on, <c>false</c> = off, <c>null</c> = this job carries no clamp info (the
    /// consumer holds the last state). Works identically on OBD/D-CAN, BT, ELM and ENET because it
    /// observes the protocol layer (the jobs and their results), not the wire.
    ///
    /// Single, authoritative source - the clamp-status text the ECU itself returns:
    ///   * STEUERN_KLEMMEN              -> STAT_CAS_KLEMMEN_STATUS_TEXT (state after the switch)
    ///   * STATUS_LESEN/STATUS_KLEMMEN  -> STAT_KLEMMENSTATUS_TEXT (live read + session baseline)
    /// "KL15" => on; "KL30..."/"KLR..." => off (a code byte 0x0A=KL15 / 0x06=KL30B, decoded by text,
    /// never a &amp; 0x01 bit test). Every other job returns null (hold).
    ///
    /// We deliberately do NOT infer a cut from the STEUERN_KL15_ABSCHALTUNG routine: ISTA emits it
    /// during BDC init (e.g. STR;3) to ARM the shutdown timer, BEFORE any real ignition-off, so it is
    /// not a reliable cut indicator (it produced false KL15-off on wireless transports where no DSR
    /// contradicts it). The real KLwechsel is fully covered by STEUERN_KLEMMEN / STATUS_KLEMMEN above.
    /// We also never infer "on" from arbitrary jobs and never read fault freeze-frames (historical).
    ///
    /// Cross-checked against ISTA 4.59.20 (DiagnosticsBusinessData.ClampShutdownManagement /
    /// VehicleIdent.SwitchClamp15) and the BDC / CAS4_2 / FEM_20 / ZGW ECU .prg definitions.
    /// </summary>
    internal static class EdiabasClampSnoop
    {
        public static bool? InferClampFromJob(string jobName, List<string> argStrings, List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
        {
            string resultName = null;
            if (string.Equals(jobName, "STEUERN_KLEMMEN", StringComparison.OrdinalIgnoreCase))
            {
                resultName = "STAT_CAS_KLEMMEN_STATUS_TEXT";
            }
            else if (string.Equals(jobName, "STATUS_LESEN", StringComparison.OrdinalIgnoreCase) && ArgContains(argStrings, "STATUS_KLEMMEN"))
            {
                resultName = "STAT_KLEMMENSTATUS_TEXT";
            }

            if (resultName == null || resultSets == null)
            {
                return null;
            }
            // Start at 1: index 0 is the system result set (job status etc.); the per-instance job
            // result sets that carry STAT_*_TEXT start at index 1.
            for (int i = 1; i < resultSets.Count; i++)
            {
                Dictionary<string, EdiabasNet.ResultData> set = resultSets[i];
                if (set != null && set.TryGetValue(resultName, out EdiabasNet.ResultData rd) && rd?.OpData is string text)
                {
                    // Documented values: KL15 | KL30B[_EIN] | KL30F_EIN | KLR_EIN. Only KL15 = on.
                    if (text.StartsWith("KL15", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (text.StartsWith("KL30", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    if (text.StartsWith("KLR", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return null;
        }

        private static bool ArgContains(List<string> argStrings, string needle)
        {
            if (argStrings == null)
            {
                return false;
            }

            for (int i = 0; i < argStrings.Count; i++)
            {
                if (string.Equals(argStrings[i], needle, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
#endif
