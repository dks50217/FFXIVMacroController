let vm = new Vue({
    mixins: [mixin],
    data() {
        return {
            loading: false,
            isInit: false,
            isStart: false,
            fullscreenLoading: false,
            form: {
                macroList: null,
                rootData : null,
                repeat: 1,
                dialogFormVisible: false,
                formLabelWidth: '120px'
            }, 
            tableColumns: [
                { label: '按鍵', prop: 'key', width: 130, type: 'select', options: 'keyOptions', optionLabel: 'label', optionValue: 'value' },
                { label: '定位', prop: 'locate', btnType: 'primary', icon: 'el-icon-position', width: 50, type: 'label', type: 'button', event: (item, scope) => { this.handleLocate(item, scope) } },
                { label: '座標', prop: 'coordinate', width: 130, type: 'label', width: 100 },
                { label: '類型', prop: 'type', width: 130, type: 'select', options: 'typeOptions', optionLabel: 'label', optionValue: 'value' },
                { label: '執行後暫停', prop: 'sleep', width: 130, type: 'input' },
                { label: '刪除', prop: 'keyName', btnType: 'danger', icon: 'el-icon-delete' , width: 50, type: 'button', event: (item, scope) => { this.handleRemove(item, scope) } }
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

            console.log(param);

            vm.callAPI("../Start", param).load.then(function (response) {
                console.log('result', response);
                _self.loading = false;
                _self.isStart = false;
            });
        },
        onStop() {
            let _self = this;
            _self.isStart = true;
            _self.loading = false;
            _self.isInit = false;

            vm.callAPI("../Stop").load.then(function (response) {
                console.log('result', response);
                _self.loading = false;
                _self.isInit = true;
            });
        },
        onInit() {
            let _self = this;
            _self.callAPI("../Init").load.then(function (response) {
                console.log('result', response);
                _self.form.rootData = response.rootData;
                _self.Options.keyOptions = response.keyOptions;
                _self.Options.typeOptions = response.typeOptions;
                _self.infoData.gamePath = response.gamePath;
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

        },
        handleAdd(macroList) {
            let _self = this;

            let param = {
                "type": 1,
                "key": 49,
                "sleep": 3000,
                "keyName": "D1",
                "coordinateX": 0,
                "coordinateY": 0,
                "coordinate" : "(0,0)"
            };

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
                text: '請到對應視窗按下ALT定位滑鼠',
                spinner: 'el-icon-coordinate',
                background: 'rgba(0, 0, 0, 0.7)'
            });

            _self.callAPI("../LocateMouse").load.then(function (response) {
                scope.row.coordinateX = response.coordinateX;
                scope.row.coordinateY = response.coordinateY;
                scope.row.coordinate = "(" + scope.row.coordinateX + "," + scope.row.coordinateY + ")";
                loading.close();
            });
        }
    },
    mounted() {
        this.onInit();
    },
    el: "#app"
})