﻿using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Streams.Kafka.E2E.Extensions;
using Orleans.Streams.Utils;
using Xunit;

namespace Orleans.Streams.Kafka.E2E.Grains
{
	public interface IStreamGrainV2 : IBaseTestGrain
	{
		Task<TestResult> Fire();
	}

	[Reentrant]
	public class RoundTripGrain : BaseTestGrain, IStreamGrainV2
	{
		private IAsyncStream<TestModel> _stream;
		private TestModel _model;
		private TaskCompletionSource<TestResult> _completion;

		public override async Task OnActivateAsync()
		{
			var provider = GetStreamProvider(Consts.KafkaStreamProvider);
			_stream = provider.GetStream<TestModel>(Consts.StreamId, Consts.StreamNamespace);

			_model = TestModel.Random();
			_completion = new TaskCompletionSource<TestResult>();

			await _stream.QuickSubscribe((actual, token) =>
			{
				_completion.SetResult(new TestResult
				{
					Actual = actual,
					Expected = _model
				});

				return Task.CompletedTask;
			});
		}

		public async Task<TestResult> Fire()
		{
			await _stream.OnNextAsync(_model);
			return await _completion.Task;
		}
	}
}
