using System;
using System.Transactions;

namespace Carbon.Core.Internals
{
    public class TransactionContext
    {
        private readonly Action m_action;
        private readonly bool m_is_transactional;

        public TransactionContext(Action action)
            :this(action, false)
        {
        
        }

        public TransactionContext(Action action, bool isTransactional)
        {
            m_action = action;
            m_is_transactional = isTransactional;
            this.RunInTransaction();
        }

        private void RunInTransaction()
        {
            if (m_is_transactional)
            {
                using (var txn = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    try
                    {
                        m_action.Invoke();
                        txn.Complete();
                    }
                    catch (Exception exception)
                    {
                        throw;
                    }
                }
            }
            else
            {
                try
                {
                    m_action.Invoke();
                }
                catch (Exception exception)
                {
                    throw;
                }
            }
        }
    }
}