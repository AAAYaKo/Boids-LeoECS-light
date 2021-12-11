using Leopotam.EcsLite;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Jobs;


namespace AAAAYaKo.EcsTransformJobs
{
	public abstract class EcsUnityTransformJobSystem<TJob, T1, TTransform> : EcsUnityJobTransformsSystemBase
		where TJob : struct, IEcsUnityTransformsJob<T1>
		where T1 : unmanaged
		where TTransform : struct
	{
		private EcsFilter _filter;
		private EcsFilter _filterTransform;
		private EcsPool<T1> _pool1;
		protected EcsPool<TTransform> _poolTransform;
		private FilterEventListener _eventListener;


		public override void Init(EcsSystems systems)
		{
			var world = GetWorld(systems);
			_pool1 = world.GetPool<T1>();
			_poolTransform = world.GetPool<TTransform>();
			_eventListener = new FilterEventListener(this);
			_filter = GetFilter(world);
			_filterTransform = world.Filter<TTransform>().End();
			_filterTransform.AddEventListener(_eventListener);
			OnEntityTransformsPoolCanged();
		}

		public override void Run(EcsSystems systems)
		{
			var nEntities = NativeHelpers.WrapToNative(_filter.GetRawEntities());
			var nDense1 = NativeHelpers.WrapToNative(_pool1.GetRawDenseItems());
			var nSparse1 = NativeHelpers.WrapToNative(_pool1.GetRawSparseItems());
			TJob job = default;
			job.Init(nEntities.Array, nDense1.Array, nSparse1.Array);
			SetData(systems, ref job);
			job.Schedule(_transformAccessArray).Complete();
#if UNITY_EDITOR
			NativeHelpers.UnwrapFromNative(nEntities);
			NativeHelpers.UnwrapFromNative(nDense1);
			NativeHelpers.UnwrapFromNative(nSparse1);
#endif
		}

		public override void Destroy(EcsSystems systems)
		{
			_filterTransform.RemoveEventListener(_eventListener);
			_transformAccessArray.Dispose();
		}

		protected override void OnEntityTransformsPoolCanged()
		{
			_transforms.Clear();
			foreach (var entity in _filter)
				_transforms.Add(GetTransform(_poolTransform.Get(entity)));
			RebuildTransformAccessArray(_transforms.ToArray());
		}

		protected abstract Transform GetTransform(TTransform transform);

		protected virtual void SetData(EcsSystems systems, ref TJob job) { }
	}

	public abstract class EcsUnityTransformJobSystem<TJob, T1, T2, TTransform> : EcsUnityJobTransformsSystemBase
		where TJob : struct, IEcsUnityJob<T1, T2>
		where T1 : unmanaged
		where T2 : unmanaged
		where TTransform : struct
	{
		private EcsFilter _filter;
		private EcsFilter _filterTransform;
		private EcsPool<T1> _pool1;
		private EcsPool<T2> _pool2;
		private EcsPool<TTransform> _poolTransform;
		private FilterEventListener _eventListener;


		public override void Init(EcsSystems systems)
		{
			var world = GetWorld(systems);
			_pool1 = world.GetPool<T1>();
			_pool2 = world.GetPool<T2>();
			_poolTransform = world.GetPool<TTransform>();
			_eventListener = new FilterEventListener(this);
			_filter = GetFilter(world);
			_filterTransform = world.Filter<TTransform>().End();
			_filterTransform.AddEventListener(_eventListener);
			OnEntityTransformsPoolCanged();
		}

		public override void Run(EcsSystems systems)
		{
			var nEntities = NativeHelpers.WrapToNative(_filter.GetRawEntities());
			var nDense1 = NativeHelpers.WrapToNative(_pool1.GetRawDenseItems());
			var nSparse1 = NativeHelpers.WrapToNative(_pool1.GetRawSparseItems());
			var nDense2 = NativeHelpers.WrapToNative(_pool2.GetRawDenseItems());
			var nSparse2 = NativeHelpers.WrapToNative(_pool2.GetRawSparseItems());
			TJob job = default;
			job.Init(nEntities.Array, nDense1.Array, nSparse1.Array, nDense2.Array, nSparse2.Array);
			SetData(systems, ref job);
			job.Schedule(_transformAccessArray).Complete();
#if UNITY_EDITOR
			NativeHelpers.UnwrapFromNative(nEntities);
			NativeHelpers.UnwrapFromNative(nDense1);
			NativeHelpers.UnwrapFromNative(nSparse1);
			NativeHelpers.UnwrapFromNative(nDense2);
			NativeHelpers.UnwrapFromNative(nSparse2);
#endif
		}

		public override void Destroy(EcsSystems systems)
		{
			_filterTransform.RemoveEventListener(_eventListener);
			_transformAccessArray.Dispose();
		}

		protected override void OnEntityTransformsPoolCanged()
		{
			_transforms.Clear();
			foreach (var entity in _filter)
				_transforms.Add(GetTransform(_poolTransform.Get(entity)));
			RebuildTransformAccessArray(_transforms.ToArray());
		}

		protected abstract Transform GetTransform(TTransform transform);

		protected virtual void SetData(EcsSystems systems, ref TJob job) { }
	}

	public abstract class EcsUnityJobTransformsSystemBase : IEcsRunSystem, IEcsInitSystem, IEcsDestroySystem
	{
		protected readonly List<Transform> _transforms = new List<Transform>();
		protected TransformAccessArray _transformAccessArray;


		public abstract void Init(EcsSystems systems);
		public abstract void Run(EcsSystems systems);
		public abstract void Destroy(EcsSystems systems);
		protected abstract EcsFilter GetFilter(EcsWorld world);
		protected abstract EcsWorld GetWorld(EcsSystems systems);
		protected abstract void OnEntityTransformsPoolCanged();
		protected void RebuildTransformAccessArray(Transform[] transforms)
		{
			if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
			_transformAccessArray = new TransformAccessArray(transforms);
		}

		public class FilterEventListener : IEcsFilterEventListener
		{
			private readonly EcsUnityJobTransformsSystemBase _system;


			public FilterEventListener(EcsUnityJobTransformsSystemBase system)
			{
				_system = system;
			}

			public void OnEntityAdded(int entity)
			{
				_system.OnEntityTransformsPoolCanged();
			}

			public void OnEntityRemoved(int entity)
			{
				_system.OnEntityTransformsPoolCanged();
			}
		}
	}

	public interface IEcsUnityTransformsJob<T1> : IJobParallelForTransform
		where T1 : unmanaged
	{
		void Init(NativeArray<int> entities, NativeArray<T1> pool1, NativeArray<int> indices1);
	}

	public interface IEcsUnityJob<T1, T2> : IJobParallelForTransform
	   where T1 : unmanaged
	   where T2 : unmanaged
	{
		void Init(
			NativeArray<int> entities,
			NativeArray<T1> pool1, NativeArray<int> indices1,
			NativeArray<T2> pool2, NativeArray<int> indices2);
	}

	static class NativeHelpers
	{
		public static unsafe NativeWrappedData<T> WrapToNative<T>(T[] managedData) where T : unmanaged
		{
			fixed (void* ptr = managedData)
			{
#if UNITY_EDITOR
				var nativeData = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, managedData.Length, Allocator.None);
				var sh = AtomicSafetyHandle.Create();
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeData, sh);
				return new NativeWrappedData<T> { Array = nativeData, SafetyHandle = sh };
#else
                return new NativeWrappedData<T> { Array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T> (ptr, managedData.Length, Allocator.None) };
#endif
			}
		}

#if UNITY_EDITOR
		public static void UnwrapFromNative<T1>(NativeWrappedData<T1> sh) where T1 : unmanaged
		{
			AtomicSafetyHandle.CheckDeallocateAndThrow(sh.SafetyHandle);
			AtomicSafetyHandle.Release(sh.SafetyHandle);
		}
#endif
		public struct NativeWrappedData<TT> where TT : unmanaged
		{
			public NativeArray<TT> Array;
#if UNITY_EDITOR
			public AtomicSafetyHandle SafetyHandle;
#endif
		}
	}
}