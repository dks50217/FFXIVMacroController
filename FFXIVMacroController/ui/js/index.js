let vm = new Vue({
    mixins: [mixin],
    data() {
        return {
            loading: false,
            isInit: false,
            isStart: false,
            saveLoader: false,
            fullscreenLoading: false,
            form: {
                macroList: null,
                rootData : null,
                repeat: 1,
                dialogFormVisible: false,
                formLabelWidth: '120px'
            }, 
            tableColumns: [
                { label: '按鍵', prop: 'keyNumber', width: 130, type: 'select', options: 'keyOptions', optionLabel: 'label', optionValue: 'value' },
                { label: '定位', prop: 'locate', btnType: 'primary', icon: 'el-icon-position', width: 50, type: 'label', type: 'button', event: (item, scope) => { this.handleLocate(item, scope) } },
                { label: '座標', prop: 'coordinate', width: 130, type: 'label', width: 100 },
                { label: '類型', prop: 'type', width: 130, type: 'select', options: 'typeOptions', optionLabel: 'label', optionValue: 'value' },
                { label: '執行後暫停', prop: 'sleep', width: 130, type: 'input' },
                { label: '文字', prop: 'inputText', width: 300, type: 'textarea' },
                { label: '刪除', prop: 'keyName', fixed: "right", btnType: 'danger', icon: 'el-icon-delete', width: 50, type: 'button', event: (item, scope) => { this.handleRemove(item, scope) } },
               /* { label: '排序', prop: 'key', btnType: 'success', type: 'button', icon: 'el-icon-rank', width: 50, class: 'drag', event: () => void(0)}*/
            ],
            Options: {
                keyOptions: [],
                typeOptions: []
            },
            infoData: {
                gamePath : ''
            }
        }
    },
    components: {
       'el-table-draggable': httpVueLoader('../component/el-table-draggable.vue'),
       'raw-displayer': httpVueLoader('../component/raw-displayer.vue')
    },
    created() {

    },
    computed: {

    },
    methods: {
        onStart(){
            let _self = this;
            _self.isStart = true;
            _self.loading = true;

            let param = _self.form.rootData.categoryList.find(c => c.id == _self.form.rootData.rootID);
            let repeat = (array, numberOfTimes) => Array(numberOfTimes).fill(array).reduce((a, b) => [...a, ...b], [])

            let apiObj = Object.assign({}, param);

            apiObj.macroList = repeat(param.macroList, _self.form.repeat);

            console.log(apiObj, repeat);

            vm.callAPI("../Start", apiObj).load.then(function (response) {
                console.log('result', response);
                _self.loading = false;
                _self.isStart = false;
            });
        },
        onStop() {
            let _self = this;
            _self.isStart = true;
            _self.isInit = false;

            vm.callAPI("../Stop").load.then(function (response) {
                console.log('result Stop', response);
                _self.loading = false;
                _self.isInit = true;
            });
        },
        onInit() {
            let _self = this;
            _self.callAPI("../Init").load.then(function (response) {
                console.log('result', response);
                _self.Options.keyOptions = response.keyOptions;
                _self.Options.typeOptions = response.typeOptions;
                _self.infoData.gamePath = response.gamePath;
                _self.form.rootData = response.rootData;
                _self.isInit = true;

                let messageType = response.gamePath ? 'success' : 'error';
                let messageShow = response.gamePath ? '初始化成功' : '初始化失敗';

                _self.$message({
                    showClose: true,
                    message: messageShow,
                    type: messageType
                });
            });
        },
        handleTabClick() {
            console.log('handleTabClick');
        },
        handleAdd(macroList) {
            let _self = this;
            let param = {};

            if (macroList.length == 0) {
                param = {
                    "type": 1,
                    "key": 49,
                    "sleep": 3,
                    "keyName": "D1",
                    "keyNumber": 49,
                    "coordinateX": 0,
                    "coordinateY": 0,
                    "coordinate": "(0,0)",
                    "inputText": "/echo 123"
                };
            }
            else {
                param = Object.assign({}, macroList[macroList.length - 1]);
            }

            macroList.push(param);
        },
        handleRemove(item, scope) {
            let _self = this;
            item.macroList.splice(scope.$index, 1);
        },
        addCategory() {
            let _self = this;
            _self.form.dialogFormVisible = true;
        },
        handleLocate(item, scope) {
            let _self = this;

            const loading = _self.$loading({
                lock: true,
                text: '請到對應視窗按下左Ctrl定位滑鼠',
                spinner: 'el-icon-coordinate',
                background: 'rgba(0, 0, 0, 0.7)'
            });

            _self.callAPI("../LocateMouse").load.then(function (response) {
                scope.row.coordinateX = response.coordinateX;
                scope.row.coordinateY = response.coordinateY;
                scope.row.coordinate = "(" + scope.row.coordinateX + "," + scope.row.coordinateY + ")";
                loading.close();

                _self.$message({
                    showClose: true,
                    message: "定位成功 " + scope.row.coordinate, 
                    type: "success"
                });
            });
        },
        onSave() {
            let _self = this;
            _self.saveLoader = true;
            _self.callAPI("../Save", _self.form.rootData).load.then(function (response) {
                _self.saveLoader = false;

                if (response) {
                    _self.$message({ message: '儲存成功', type: 'success' });
                }
                else {
                    _self.$message.error('儲存失敗');
                }
            });
        }
    },
    mounted() {
        this.onInit();
    },
    el: "#app"
})