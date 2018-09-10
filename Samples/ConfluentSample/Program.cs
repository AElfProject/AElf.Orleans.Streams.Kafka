﻿using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfluentSample
{
	internal class Program
	{
		private static async Task Main(string[] args)
		{
			Task.Run(() => Consume());

//			Task.Run(async () =>
//			{
//				while (true)
//				{
//					await Produce();
//					await Task.Delay(100);
//				}
//			});

			Console.ReadKey();
		}

		private static async Task Produce()
		{
			var config = new Dictionary<string, object>
			{
				{ "bootstrap.servers", "localhost:9092" }
			};

			using (var producer = new Producer<string, string>(
				config,
				new StringSerializer(Encoding.UTF8),
				new StringSerializer(Encoding.UTF8))
			)
			{
				var publishPromise3 = producer.ProduceAsync("my-topic", new Message<string, string>
				{
					Key = "jello",
					Value = "jelo jello jelllo the world"
				});

				var publishPromise2 = producer.ProduceAsync("my-topic", new Message<string, string>
				{
					Key = "hello",
					Value = "helloing the world"
				});

				var publishPromise = producer.ProduceAsync("my-topic", new Message<string, string>
				{
					Key = "jonny",
					Value = "test message text"
				});

				var publishPromise4 = producer.ProduceAsync("my-topic", new Message<string, string>
				{
					Key = "dog-dragons",
					Value = "dogzinskis"
				});

				await Task.WhenAll(publishPromise, publishPromise2, publishPromise3, publishPromise4);

				Console.WriteLine($@"Delivered '{publishPromise.Result.Value}' to: {publishPromise.Result.TopicPartitionOffset}");
				Console.WriteLine($@"Delivered '{publishPromise2.Result.Value}' to: {publishPromise2.Result.TopicPartitionOffset}");
				Console.WriteLine($@"Delivered '{publishPromise3.Result.Value}' to: {publishPromise3.Result.TopicPartitionOffset}");
				Console.WriteLine($@"Delivered '{publishPromise4.Result.Value}' to: {publishPromise4.Result.TopicPartitionOffset}");
			}
		}

		private static void Consume()
		{
			var conf = new Dictionary<string, object>
			{
				{ "group.id", "test-consumer-group" },
				{ "bootstrap.servers", "localhost:9092" },
				{ "enable.auto.commit", false },
				//{ "auto.commit.interval.ms", 5000 },
//				{ "auto.offset.reset", "earliest" }
			};

			using (var consumer = new Consumer<string, string>(conf, new StringDeserializer(Encoding.UTF8), new StringDeserializer(Encoding.UTF8)))
			using (var admin = new AdminClient(consumer.Handle))
			{
				Console.WriteLine($@"Partition IDs: {
						string.Join(',',
						admin
						.GetMetadata(TimeSpan.FromMilliseconds(1000))
						.Topics
						.First(t => t.Topic.Contains("my-topic"))
						.Partitions
						.Select(x => x.PartitionId))
					}"
				);

				consumer.OnPartitionEOF += (_, end)
					=> Console.WriteLine($"Reached end of topic {end.Topic} partition {end.Partition}, next message will be at offset {end.Offset}");
				

				consumer.Assign(new TopicPartitionOffset("my-topic", 1, Offset.Beginning));
				//consumer.Subscribe("my-topic");

				while (true)
				{
					var message = consumer.Consume(TimeSpan.FromMilliseconds(100));
					if (message != null)
						Console.WriteLine(message.Value);
				}
			}
		}
	}
}