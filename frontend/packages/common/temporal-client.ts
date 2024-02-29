import { Connection, Client, ConnectionOptions } from '@temporalio/client'
import { getConnectionOptions, namespace } from './temporal-connection'

export async function connectToTemporal() {
  const connectionOptions: ConnectionOptions = await getConnectionOptions();
  return new Client({
    connection: await Connection.connect(connectionOptions).catch((err) => {
      console.error('Error connecting to Temporal Server: ', err)
      return undefined
    }),
    namespace,
  })
}
