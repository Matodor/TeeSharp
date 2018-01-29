using TeeSharp.Common.Protocol;

namespace TeeSharp.Common.Game
{
    public class CharacterCore
    {
        public virtual Vec2 Position { get; set; }

        public virtual SnapObj_PlayerInput Input { get; set; }

        protected virtual WorldCore World { get; set; }

        public void Init(WorldCore worldCore)
        {
            World = worldCore;
        }

        public void Reset()
        {
            
        }

        public void Tick(bool useInput)
        {
            
        }

        public void FillTo(CharacterCore output)
        {
            /*
             * output.m_Pos = m_Pos;
               output.m_Vel = m_Vel;
               output.m_HookPos = m_HookPos;
               output.m_HookDir = m_HookDir;
               output.m_HookTick = m_HookTick;
               output.m_Angle = m_Angle;
               output.m_Direction = m_Direction;
               output.m_HookState = m_HookState;
               output.m_HookedPlayer = m_HookedPlayer;
               output.m_Jumped = m_Jumped;
               output.m_TriggeredEvents = m_TriggeredEvents;
               m_Input.Write(output.m_Input);
             **/
        }

        public void Move()
        {
            throw new System.NotImplementedException();
        }

        public void Quantize()
        {
            throw new System.NotImplementedException();
        }

        public void Write(SnapObj_Character character)
        {
            throw new System.NotImplementedException();
        }
    }
}