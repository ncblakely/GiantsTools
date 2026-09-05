using System;

namespace Giants.Launcher
{
    public sealed class UpdateProgressEventArgs : EventArgs
    {
        public UpdateProgressEventArgs(
            int progressPercentage,
            long bytesReceived,
            long totalBytesToReceive,
            object userState)
        {
            this.ProgressPercentage = progressPercentage;
            this.BytesReceived = bytesReceived;
            this.TotalBytesToReceive = totalBytesToReceive;
            this.UserState = userState;
        }

        public int ProgressPercentage { get; }

        public long BytesReceived { get; }

        public long TotalBytesToReceive { get; }

        public object UserState { get; }
    }
}
