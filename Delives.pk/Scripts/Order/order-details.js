var orderDetails = {
    updateOrderItems: function () {

        var comm = $("#chageComments").val().trim();
        if (comm == "" || comm.length <10) {
            $("#chageComments").focus();
            alert("Add proper comments");
            return;
        }
        $("#loading").show();
        var items = $(".edit-item-input");
        if (items != undefined && items.length > 0) {
            var data = {
                OrderId: editOrderId,
                Comments: comm,
                Items:[]
            };
            for (var i = 0; i < items.length; i++) {
                var quant = $(items[i]).val();
                var itemId = $(items[i]).attr('data-itemId');
                data.Items.push({ itemId: itemId, quantity: quant });
            }

            //#region statuses
            $.ajax({
                method: "POST",
                url: "../AlterOrder",
                dataType: "JSON",
                data: JSON.stringify(data),
                contentType: "application/json; charset=utf-8"
            }).done(function (response) {
                if (response != null && response != undefined) {
                    if (response.isSuccesss) {
                        toastr.success("Order updated");
                        document.location.reload()
                    }
                    else {
                        toastr.error(response.Message);
                    }                    
                }

            }).fail(function (jqXHR, textStatus, errorThrown) {
                toastr.error(errorThrown);
            }).always(function () {
                $("#loading").hide();
            });
        }
    }
};