using UnityEngine;

namespace UserInTheBox
{
    public static class UitBUtils
    {

        public static string GetKeywordArgument(string argName)
        {
            // Get argName from command line arguments.
            // Throws an ArgumentException if argName is not found from command line arguments.
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == ("-" + argName) && (args.Length > i + 1))
                {
                    return args[i + 1];
                }
            }

            throw new System.ArgumentException("Could not find " + argName + " from command line arguments");
        }

        public static bool GetOptionalArgument(string argName)
        {
            // Get argName from command line arguments.
            // Returns false if argName is not found.
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == ("-" + argName))
                {
                    return true;
                }
            }

            return false;
        }

        public static string TransformToString(Transform transform)
        {
            return Vector3ToString(transform.position) + ", " + QuaternionToString(transform.rotation);
        }

        public static string Vector3ToString(Vector3 vec)
        {
            return vec.x + "," + vec.y + "," + vec.z;
        }

        public static string QuaternionToString(Quaternion quat)
        {
            return quat.x + ", " + quat.y + ", " + quat.z + ", " + quat.w;
        }
    }
}