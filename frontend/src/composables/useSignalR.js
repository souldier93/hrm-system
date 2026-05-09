import * as signalR from '@microsoft/signalr'
import { ElNotification } from 'element-plus'

let connection = null

export function useSignalR() {
    function connect(token) {
        connection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5000/hubs/notification', {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build()

        // Lắng nghe thông báo lương
        connection.on('SalaryCalculated', (message) => {
            ElNotification({
                title: '💰 Thông báo lương',
                message,
                type: 'success',
                duration: 5000
            })
        })

        connection.start().catch(console.error)
    }

    function disconnect() {
        connection?.stop()
    }

    return { connect, disconnect }
}