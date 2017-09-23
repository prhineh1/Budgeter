
var BudgetIdDelete
function GetBudgetId(selector) {
    var currentPanel;
    var budgetIdJs;
    currentPanel = $(selector).parents('div.panel.panel-info')
    BudgetIdDelete = currentPanel.children('#BudgetId').val()
    return BudgetIdDelete
}

function BudgetAjaxGet(urlTarget, selector, BudgetItemIdJs) {
    if (selector != null) {
        return $.ajax({
            url: urlTarget,
            data: {
                id: GetBudgetId(selector)
            },
            type: 'GET',
            dataType: "json",
        })
    }
    else {
        return $.ajax({
            url: urlTarget,
            data: {
                id: BudgetItemIdJs
            },
            type: 'GET',
            dataType: "text",
        })
    }
}

function BudgetAjaxPost(urlTarget, name, amount, id) {
    return $.ajax({
        url: urlTarget,
        data: {
            Name: name,
            Amount: amount,
            Id: id
        },
        type: 'POST',
        dataType: 'json',
        success: null
    });
};

/*
*
*
* DELETE BUDGET 
*
*/


$('.deleteBudgetButton').click(function () {
    $('#deleteBudgetItemForm').find('#BudgetItemDeleteListDropdown').empty();
    $('#deleteBudgetItemForm').css('display', 'initial');
    BudgetAjaxGet('/Budgets/delete/', this).done(function (json) {
        $('#DeleteBudgetName').html(json.name);
        $('#deleteBudgetButton2').val(BudgetIdDelete);
        if (json.budgetItemIdsList.length == 0) {
            $('#deleteBudgetItemForm').css('display', 'none');
        }
        else {
            for (i = 0; i < json.budgetItemNamesList.length; i++) {
                if (i == 0) {
                    $('#BudgetItemDeleteListDropdown').append("<select multiple id='budgetItemsSelectDelete' name='Ids'><option value=" + json.budgetItemIdsList[i] + ">" + json.budgetItemNamesList[i] + "</option></select>")
                }
                else {
                    $('#budgetItemsSelectDelete').append("<option value=" + json.budgetItemIdsList[i] + ">" + json.budgetItemNamesList[i] + "</option>")
                }
            };
            $('#budgetItemsSelectDelete').multiSelect({
                selectableHeader: "<div class='custom-header' style='text-align:center; font-size:1.2em'>Available</div>",
                selectionHeader: "<div class='custom-header' style='text-align:center; font-size:1.2em'>Delete</div>"
            });
        }
        //open modal
        $('#DeleteBudgetModal').modal('toggle');
    });
})

/*
*
*
* EDIT BUDGET 
*
*/
var BudgetId;
var parent;
var oldBudgetAmount;
var BudgetItemIdJs
var oldBudgetItemName
$('.editBudgetButton').click(function () {
    parent = $(this).parents('div.panel.panel-info')
    BudgetAjaxGet('/Budgets/edit', this, null).done(function (data) {
        $('#BudgetItemEditDropdown').empty();
        $('#EditBudgetName').html(data.name);
        oldBudgetAmount = data.amount;
        $('#EditBudgetAmount').html('Allocated Budget: $' + data.amount);
        BudgetId = data.Id;
        BudgetItemIdJs = data.budgetItemsIdEdit[0];
        oldBudgetItemName = data.budgetItemsNameEdit[0];
        $('#editBudgetItemName').html('Budget Item: ' + data.budgetItemsNameEdit[0]);
        $('#editBudgetItemNameInput').val(data.budgetItemsNameEdit[0]);
        for (i = 0; i < data.budgetItemsNameEdit.length; i++) {
             $('#BudgetItemEditDropdown').append("<option value='" + data.budgetItemsIdEdit[i] + "'>" + data.budgetItemsNameEdit[i] + "</option>")
        }
        $('#EditBudgetModal').modal('toggle');
    })
})


function SetItemInput(obj) {
    BudgetItemIdJs = $(obj).val();
    BudgetAjaxGet('/BudgetItems/Edit', null, BudgetItemIdJs).done(function (name) {
        oldBudgetItemName = name;
        $('#editBudgetItemNameInput').val(name);
        $('#editBudgetItemName').html('Budget Item: ' + name);
    })
}

var BudgetItemName;
$('#EditBudgetItemNameButton').click(function () {
    BudgetItemName = $('#editBudgetItemNameInput').val();
    BudgetAjaxPost('/BudgetItems/edit', BudgetItemName, null, BudgetItemIdJs).done(function (object) {
        $('#editBudgetItemName').html('Budget Item: ' + object.name);
        $('#editBudgetItemNameInput').val(object.name)
        $("#BudgetItemEditDropdown").children().each(function () {
            if ($(this).val() == object.Id) {
                $(this).html(object.name)
            }
        });
        parent.find('div.panel-body div.row div.col-md-7').children().each(function () {
            if ($(this).text() == oldBudgetItemName) {
                $(this).html(object.name)
            }
        })
        if (object.over > 0) {

        }
    })
})


var BudgetName;
var BudgetAmount;
$('#EditBudgetAndAmountButton').click(function () {
    BudgetName = $('#editBudgetNameInput').val();
    if ($('#editBudgetAmountInput').val() == '') {
        BudgetAmount = oldBudgetAmount;
    }
    else {
        BudgetAmount = $('#editBudgetAmountInput').val();
    }   
    BudgetAjaxPost('/Budgets/edit', BudgetName, BudgetAmount, BudgetId).done(function (object) {
        $('#editBudgetNameInput').val('');
        $('#editBudgetAmountInput').val('');
        $('#EditBudgetName').html(object.name);
        $('#EditBudgetAmount').html('Allocated Budget: $' + object.amount);
        if (object.over > 0) {
            parent.find('div.panel-heading h4.panel-title').html(object.name + ' <i class="fa fa-exclamation animated infinite bounce" aria-hidden="true" style="color:red;"></i>');
        }
        else {
            parent.find('div.panel-heading h4.panel-title').html(object.name)
        }
        parent.find('div.panel-heading div.text-info').html('Allocated Budget: $' + object.amount);
    })
})

/*
*
*
* ADD BUDGET ITEM
*
*/

var panelAdd;
$('.AddBudgetItemButton').click(function () {
    $('#AddBudgetItem').modal('toggle');
    panelAdd = this;
})

var itemSet2 = [];
//push entered input into itemSet
$('#createBudgetItem2').click(function () {
    if ($('#budgetItemInpu2').val().trim(" ") != "") {
        itemSet2.push($('#budgetItemInpu2').val());
    };
    //display elements of item set on page
    for (i = 0; i < itemSet2.length; i++) {
        if (i == 0) {
            $('#budgetItemList2').html('<span onclick="RemoveFromItemSetAdd(this)"><br />' + itemSet2[i] + '</span>');
        }
        else {
            $('#budgetItemList2').append('<span onclick="RemoveFromItemSetAdd(this)"><br />' + itemSet2[i] + '</span>')
        }
    }

    //Reset the input after each entry and update form action
    $('#budgetItemInpu2').val("")
    $('#BudgetItemCreateForm').prop('action', 'BudgetItems/create?BudgetId=' + GetBudgetId(panelAdd) + '&BudgetItems=' + itemSet2)
    ChangeCursor2()
});

function ChangeCursor2() {
    $('#budgetItemList2').children().each(function () {
        $(this).css('cursor', 'default')
    })
}

function RemoveFromItemSetAdd(obj) {
    obj.remove();
    var removed = itemSet2.splice(itemSet2.indexOf($(obj).text()), 1);
    $('#BudgetItemCreateForm').prop('action', 'BudgetItems/create?BudgetId=' + GetBudgetId(panelAdd) + '&BudgetItems=' + itemSet2)
}

/*
*
*
* BAR CHARTS
*
*/

//returns as a callback the results of the ajax request
//to be used in the .done() method
function barGraphAjaxGet(budgetId, householdId) {
    return $.ajax({
        url: '/Households/BarGraphInfo',
        data: {
            BudgetId: budgetId,
            HouseholdId: householdId
        },
        type: 'GET',
        dataType: "json",
    });
}

//updates the necessay properties of the chart
function changeData(chart, label, data, title, transactionMax, budgetMax) {
    chart.data.labels = label;
    chart.data.datasets = data;
    chart.options.title.text = title
    var max = Math.max(transactionMax, budgetMax);
    chart.options.scales.yAxes[0].ticks.max = max + (Math.sqrt(max) * 2.5)
    chart.update();
}

//the chart that will display on page load
var chart;
//handle Ajax callback
barGraphAjaxGet(null, householdId).done(function (object) {
    var max = Math.max(object.transactions, object.budget);
    var ctx = document.getElementById('budgetGraphs').getContext('2d');
    //construct chart
    chart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ["This Month"],
            datasets: [{
                label: "Combined Expenses",
                backgroundColor: '#e01111',
                data: [object.transactions]
            }, {
                label: "Combined Budget",
                backgroundColor: '#0c48d3',
                data: [object.budget]
            }]
        },
        options: {
            responsive: true,
            legend: {
                position: 'top',
            },
            title: {
                display: true,
                text: 'Overall Budget and Expenses'
            },
            scales: {
                yAxes: [{
                    scaleLabel: {
                        display: true,
                        labelString: "U.S. Dollars"
                    },
                    ticks: {
                        beginAtZero: true,
                        max: max + (Math.sqrt(max) * 2.5)
                    }
                }]
            },
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, chart) {
                        return " $" + tooltipItem.yLabel;
                    }
                }
            }
        }
    });
});

//chart that results from selecting an
//option in the dropdown list
//"Graphdata" will replace the "data" field in the "chart" object
var BudgetIdGraph;
var Graphdata;
//.change() triggers when the value of <select> changes
//passing the Budget Id to the Ajax call
$('#BudgetDropdownChart').change(function () {
    $(this).children().each(function () {
        if ($(this).prop('selected')) {
            BudgetIdGraph = $(this).val();
        }
    });
    //Handle Ajax callback
    barGraphAjaxGet(BudgetIdGraph, householdId).done(function (object) {
            //sets Graph data for "income vs expenses" option
            if (object.hasOwnProperty('income')) {
            Graphdata = {
                labels: ["This Month"],
                datasets: [{
                    label: "Combined Expenses",
                    backgroundColor: '#e01111',
                    data: [object.transactions]
                }, {
                    label: "Income",
                    backgroundColor: '#0c48d3',
                    data: [object.income]
                }]
            };
            var transactionSum = object.transactions;
            var maxBudget = object.income;
        }
         //sets Graph data for "All Budgets" option
        else if (object.hasOwnProperty('transactions')) {
            Graphdata = {
                labels: ["This Month"],
                datasets: [{
                    label: "Combined Expenses",
                    backgroundColor: '#e01111',
                    data: [object.transactions]
                }, {
                    label: "Combined Budget",
                    backgroundColor: '#0c48d3',
                    data: [object.budget]
                }]
            };
            var transactionSum = object.transactions;
            var maxBudget = object.budget;
        }
        //sets Graph data for any Budget option
        else {
            //the length of budgetItemList and transactionList
            //corresponds to the number of BudgetItems
            //we iterate over each budget item, creating objects with the
            //appropriate properties and them push that object into 'datasetsToAdd'
            var datasetsToAdd = [];
            for (i = 0; i < object.budgetItemList.length; i++) {
                var datasetItemExpense = {
                    label: object.budgetItemList[i],
                    data: [object.transactionsList[i]],
                    stack: 'stack 0',
                    backgroundColor: randomColor()
                };
                datasetsToAdd.push(datasetItemExpense);
            }
            //the second bar on the graph is for the budgeted Amount
            //also pushed to 'datasetsToAdd'
            var datasetItemBudget = {
                label: 'Budget',
                data: [object.budgetAmount],
                stack: 'stack 1',
                backgroundColor: randomColor()
            };
            datasetsToAdd.push(datasetItemBudget);
            //add 'datasetsToAdd' to 'Graphdata'
            //set the x-label as "This Month"
            Graphdata = {
                labels: ["This Month"],
                datasets: datasetsToAdd
            };
            var transactionSum = object.transactionsList.reduce(function (a, b) { return a + b; }, 0);
            var maxBudget = object.budgetAmount
        }
        //call the function to upgrade the existing chart
        changeData(chart, Graphdata.labels, Graphdata.datasets, object.title, transactionSum, maxBudget);
    });
});
