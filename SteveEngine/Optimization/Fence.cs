using System;
using OpenTK.Graphics.OpenGL4;

namespace SteveEngine
{
    public class Fence
    {
        private IntPtr fenceSync;
        private bool isCreated = false;

        public Fence()
        {
            Create();
        }

        public void Create()
        {
            if (!isCreated)
            {
                fenceSync = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0);
                isCreated = true;
            }
        }

        public void Insert()
        {
            if (isCreated)
            {
                // Delete the previous fence before creating a new one
                GL.DeleteSync(fenceSync);
                fenceSync = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0);
            }
            else
            {
                Create();
            }
        }

        public bool IsSignaled()
        {
            if (!isCreated) return true;

            // Using the correct method to check sync status in OpenTK
            int[] values = new int[1];
            GL.GetSync(fenceSync, SyncParameterName.SyncStatus, 1, out _, values);
            return values[0] == (int)All.Signaled;
        }

        public void WaitUntilSignaled()
        {
            if (!isCreated) return;

            // Wait for the sync object using ClientWaitSync
            GL.ClientWaitSync(fenceSync, ClientWaitSyncFlags.SyncFlushCommandsBit, 1000000000); // 1 second timeout
        }

        public void Delete()
        {
            if (isCreated)
            {
                GL.DeleteSync(fenceSync);
                isCreated = false;
            }
        }
    }
}