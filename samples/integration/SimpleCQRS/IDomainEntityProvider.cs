using System;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using SimpleCQRS.Storage;

namespace SimpleCQRS
{
    public interface IDomainEntityProvider<TEntity> where TEntity : BaseDomainEntity
    {
        TEntity Create<TEntity>(IAggregateRoot root) where TEntity : BaseDomainEntity, new();
    }

    public class DomainEntityProvider<TEntity> : IDomainEntityProvider<TEntity> where TEntity : BaseDomainEntity
    {
        public TEntity Create<TEntity>(IAggregateRoot<TEnti root) where TEntity : BaseDomainEntity, new()
        {
            var entity = new TEntity();

            if (entity != null)
                entity.Parent = root;

            return entity;
        }

        public TEntity Create<TEntity>(IAggregateRoot root, object[] constructorArgs) where TEntity : BaseDomainEntity, new()
        {
            var proxy = new ProxyGenerator();

            //proxy.CreateClassProxy(typeof (TEntity), typeof (DomainEntityMethodInterceptor));
            //proxy.CreateClassProxy(typeof(TEntity))
             
            var entity = Activator.CreateInstance(typeof (TEntity), constructorArgs) as BaseDomainEntity;

            if (entity != null)
                entity.Parent = root;

            return (TEntity)entity;
        }
    }

    public class DomainEntityMethodInterceptor : IInterceptor
    {
        private readonly IEventStorage _storage;

        public  DomainEntityMethodInterceptor(IEventStorage storage)
        {
            _storage = storage;
        }

        public void Intercept(IInvocation invocation)
        {
            if(invocation.Method.Name.StartsWith("_set"))
            {
               // _storage.
            }
        }
    }
}