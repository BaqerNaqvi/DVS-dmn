
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
                ${order.RestName}
            </td>
            <td>
                ${order.Amount}
            </td>
              <td class ="lastTd">
                    <button class ="btn btn-primary"><a style="color:white" href="/AdOrders/Edit/${order.Id}">Edit</a></button>
                    <button class ="btn btn-default"><a style="color:black" href="/AdOrders/Details/${order.Id}">Details</a></button>
                </td>            
        </tr>`;
            toastr.options.fadeOut = 5000;
            toastr.success(`New order received, to deliver on ${order.Address}`);
            var audio = new Audio(soundNotificationFileUrl);
            const playPromise = audio.play();
            if (playPromise !== null) {
                playPromise.catch(() => { media.play(); })
            }
        });
        $('#allOrdersTable tbody tr:first').before(ordersHTML);

    },
    getFiltersData: () => {

        try {
            //#region statuses
            $.ajax({
                method: "GET",
                url: "GetAllOrderStatuses",
                dataType: "JSON",
                contentType: "application/json; charset=utf-8"
            }).done(function (response) {
                //var cc = response;

                if (response.Success === true) {
                    let output = [];

                    $.each(response.ListStatuses, function (key, value) {
                        output.push('<option value="' + value.Value + '">' + value.Value + '</option>');
                    });

                    $('#order-status-input').html(output.join(''));

                    bindMultiselect($('#order-status-input'));
                } else {
                    console.log(response.Message);
                }

            }).fail(function (jqXHR, textStatus, errorThrown) {
                toastr.error(errorThrown);
            }).always(function () {
                //alert("complete");
            });
            //#endregion


            //#region restaurants
            $.ajax({
                method: "GET",
                url: "GetAllRestaurants",
                dataType: "JSON",
                contentType: "application/json; charset=utf-8"
            }).done(function (response) {
                if (response.Success === true) {
                    let output = [];

                    $.each(response.ListRestaurants, function (key, value) {
                        output.push('<option value="' + value.Id + '">' + value.Name + '</option>');
                    });

                    $('#order-res-input').html(output.join(''));
                    bindMultiselect($('#order-res-input'));
                } else {
                    console.log(response.Message);
                }
            }).fail(function (jqXHR, textStatus, errorThrown) {
                toastr.error(errorThrown);
            }).always(function () {
                //alert("complete");
            });
            //#endregion

            //#region riders
            $.ajax({
                method: "GET",
                url: "GetAllRiders",
                dataType: "JSON",
                contentType: "application/json; charset=utf-8"
            }).done(function (response) {
                if (response.Success === true) {
                    let output = [];

                    $.each(response.ListRiders, function (key, value) {
                        output.push('<option value="' + key + '">' + value + '</option>');
                    });

                    $('#order-delivered-by-input').html(output.join(''));
                    bindMultiselect($('#order-delivered-by-input'));
                } else {
                    console.log(response.Message);
                }
            }).fail(function (jqXHR, textStatus, errorThrown) {
                toastr.error(errorThrown);
            }).always(function () {
                //alert("complete");
            });
            //#endregion

        } catch (e) {
            console.log(e);
        }
    },
    searchOrders: () => {

        const objToServer = {
            ListStatuses: $("#order-status-input").val(),
            ListRestaurants: $("#order-res-input").val(),
            ListRiders: $("order-delivered-by-input").val()
        };

        //showProgress();
        var jqxhr = $.post("SearchOrders", objToServer, function (obj) {
            //hideProgress()
            if (obj !== null) {
                $("#all-orders-table").html(response);
            } else {
                toastr.error("could not search orders, please try again or contact Delivers");
            }
        }
        ).fail(function (err) {
            //hideProgress()
            console.log(err);
        });
    }   
}

const bindMultiselect = (obj) => {

    $(obj).multiselect();
};


$(document).ready(() => {

    //orderMainJsObj.getFiltersData();
});




setInterval(function () {
    location.reload();
}, 30000);