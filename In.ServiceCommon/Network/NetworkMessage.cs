using System;

namespace In.ServiceCommon.Network
{
    

    public class NetworkMessage
    {
        private int _offset;
        private byte[] _message;

        public NetworkMessage(int length)
        {
            _offset = 0;
            _message = new byte[length];
        }

        public void AppendData(byte[] buffer, int count)
        {
            Array.Copy(buffer, 0, _message, _offset, count);
            _offset += count;
        }

        public byte[] GetBytes()
        {
            return _message;
        }
    }
}