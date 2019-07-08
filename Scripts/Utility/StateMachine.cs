/******************************************************************************/
/*!    \brief  ステートマシン.
*******************************************************************************/

using System;
using System.Collections.Generic;
namespace VMUnityLib
{
    public class StateMachine<T>
    {
        /// <summary>
        /// ステート.
        /// </summary>
        private class State
        {
            private readonly Action EnterAct;  // 開始時に呼び出されるデリゲート.
            private readonly Action UpdateAct; // 更新時に呼び出されるデリゲート.
            private readonly Action ExitAct;   // 終了時に呼び出されるデリゲート.

            /// <summary>
            /// コンストラクタ.
            /// </summary>
            public State(Action enterAct = null, Action updateAct = null, Action exitAct = null)
            {
                EnterAct = enterAct ?? delegate { };
                UpdateAct = updateAct ?? delegate { };
                ExitAct = exitAct ?? delegate { };
            }

            /// <summary>
            /// 開始します.
            /// </summary>
            public void Enter()
            {
                EnterAct();
            }

            /// <summary>
            /// 更新します.
            /// </summary>
            public void Update()
            {
                UpdateAct();
            }

            /// <summary>
            /// 終了します.
            /// </summary>
            public void Exit()
            {
                ExitAct();
            }
        }

        private Dictionary<T, State> StateTable = new Dictionary<T, State>();   // ステートのテーブル.
        private State CurrentState;                                             // 現在のステート.
        private T CurrentStateKey;                                              // 現在のステートキー.

        /// <summary>
        /// ステートを追加します.
        /// </summary>
        public void Add(T key, Action enterAct = null, Action updateAct = null, Action exitAct = null)
        {
            StateTable.Add(key, new State(enterAct, updateAct, exitAct));
        }

        /// <summary>
        /// 現在のステートを設定します.
        /// </summary>
        public void SetState(T key)
        {
            if (CurrentState != null)
            {
                CurrentState.Exit();
            }
            CurrentStateKey = key;
            CurrentState = StateTable[key];
            CurrentState.Enter();
        }

        /// <summary>
        /// 現在のステートを取得します.
        /// </summary>
        public T GetState()
        {
            return CurrentStateKey;
        }


        /// <summary>
        /// 現在のステートを更新します.
        /// </summary>
        public void Update()
        {
            if (CurrentState == null)
            {
                return;
            }
            CurrentState.Update();
        }

        /// <summary>
        /// すべてのステートを削除します.
        /// </summary>
        public void Clear()
        {
            StateTable.Clear();
            CurrentState = null;
        }
    }
}