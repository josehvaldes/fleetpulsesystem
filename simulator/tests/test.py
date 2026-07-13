import time
from paho.mqtt import client as mqtt_client

BROKER = 'localhost'
PORT = 1883
TOPIC = "fleet_pulse"
CLIENT_ID = 'py_sim_paho_v2'


def on_connect(client, userdata, flags, rc, properties=None):
    """Callback triggered when the client connects to the broker."""
    if rc == 0:
        print("Successfully connected to MQTT Broker!")
        # Subscribe to the topic upon successful connection
        client.subscribe(TOPIC)
    else:
        print(f"Failed to connect, return code {rc}")

def on_message(client, userdata, msg):
    """Callback triggered when a message is received on a subscribed topic."""
    print(f"Received message: `{msg.payload.decode()}` from topic: `{msg.topic}`")

def run():
    # 1. Instantiate the client with API Version 2 (Mandatory in paho-mqtt 2.x)
    client = mqtt_client.Client(
        callback_api_version=mqtt_client.CallbackAPIVersion.VERSION2, 
        client_id=CLIENT_ID
    )
    
    # 2. Assign the callback functions
    client.on_connect = on_connect
    client.on_message = on_message

    # 3. Connect to the designated MQTT broker
    client.connect(BROKER, PORT, keepalive=60)
    

    # 4. Start the network loop in a non-blocking background thread
    client.loop_start()

    try:
        # 5. Periodically publish messages
        msg_count = 1
        while msg_count <= 1:  # Limit to 1 message for demonstration
            time.sleep(2)
            msg = f"1_Message number {msg_count}"
            result = client.publish(TOPIC, msg, qos=1)
            print(f"Published message: `{msg}` to topic: `{TOPIC}`")
            print(f"result: {result}")
            status = result[0]
            # Verify if the message was sent successfully
            if status == mqtt_client.MQTT_ERR_SUCCESS:
                print(f"Sent: `{msg}`")
            else:
                print(f"Failed to send message {msg_count}")
            msg_count += 1
            
    except KeyboardInterrupt:
        print("Disconnecting gracefully...")
    finally:
        # 6. Stop the network loop and disconnect cleanly
        client.loop_stop()
        client.disconnect()


if __name__ == '__main__':
    run()