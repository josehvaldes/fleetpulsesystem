from confluent_kafka.admin import (
    AdminClient,
    NewTopic
)

print("Connecting to Redpanda localhost:19092...")
admin = AdminClient({
    "bootstrap.servers": "localhost:19092"
})

topics = [
    NewTopic('gps-pings', num_partitions=3, replication_factor=1),
    NewTopic('ai-alerts', num_partitions=1, replication_factor=1)
]

print("Creating topics...")
futures = admin.create_topics(topics)

for topic_name, future in futures.items():
    try:
        future.result()
        print(f"Created {topic_name}")
    except Exception as ex:
        print(f"Failed: {ex}")


print("Checking if topic exists...")
metadata = admin.list_topics(timeout=5)

if "gps-pings" in metadata.topics:
    print("Topic 'gps-pings' exists")
else:
    print("Topic 'gps-pings' missing")

if "ai-alerts" in metadata.topics:
    print("Topic 'ai-alerts' exists")
else:
    print("Topic 'ai-alerts' missing")