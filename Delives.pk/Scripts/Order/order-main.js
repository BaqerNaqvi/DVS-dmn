
$(function () {
    // Reference the auto-generated proxy for the hub.  
    var chat = $.connection.notifierHub;
    // Create a function that the hub can call back to display messages.
    chat.client.addNewOrdersToPage = function (orders) {
        orderMainJsObj.paintNewOrders(orders);
    };

    // Start the connection.
    $.connection.hub.start().done(function () {
        //regisgter user here for future purposes!
    });

    $.connection.hub.disconnected(function () {
        console.log("Connection disconnected");
        setTimeout(function () {
            console.log("Trying to restart signalr...");
            $.connection.hub.start();
        }, 5000);
    });
});

const soundNotificationFileUrl = `${window.location.origin}/content/short_tone.mp3`; 

const orderMainJsObj = {
    paintNewOrders: (orders) => {

        let ordersHTML = ``;
        orders.forEach((order) => {
            ordersHTML += `<tr class='newOrder'>
            <td>
                ${order.Id}
            </td>
            <td>
                ${order.SerialNo}
            </td>
            <td>
                ${order.DateTime}
            </td>
            <td>
                ${order.Status}
            </td>
            <td>
                ${order.Address}
            </td>
            <td>
                ${order.Amount}
            </td>
            <td>
                ${order.EstimatedTime}
            </td>
          
            <td>
                ${order.PickedBy}
            </td>

           
            <td>
                <a href="/AdOrders/Edit/${order.Id}">Edit</a> |
                <a href="/AdOrders/Details/${order.Id}">Details</a> |
                <a href="/AdOrders/Delete/${order.Id}">Delete</a>
            </td>
        </tr>`;
            toastr.options.fadeOut = 5000;
            toastr.success(`New order received, to deliver on ${order.Address}`);
            var audio = new Audio(soundNotificationFileUrl);
            const playPromise= audio.play();
            if (playPromise !== null) {
                playPromise.catch(() => { media.play(); })
            }
        });
        $('#allOrdersTable tbody tr:first').before(ordersHTML);

    }
}

//setTimeout(function () {
//    location.reload();
//}, 30000);