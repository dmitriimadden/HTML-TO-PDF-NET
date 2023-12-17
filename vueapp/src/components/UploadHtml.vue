<template>
    <div id="app">
        <div class="container">
            <!--UPLOAD-->
            <form enctype="multipart/form-data" novalidate v-if="isInitial || isSaving">
                <h1>Upload HTML</h1>
                <div class="dropbox">
                    <input type="file" multiple :name="uploadFieldName" :disabled="isSaving" @change="filesChange($event.target.name, $event.target.files); fileCount = $event.target.files.length"
                           accept="html/*" class="input-file">
                    <p v-if="isInitial">
                        Drag your file(s) here to begin<br> or click to browse
                    </p>
                    <p v-if="isSaving">
                        Uploading {{ fileCount }} files...
                    </p>
                </div>
            </form>
            <!--SUCCESS-->
            <div v-if="isSuccess">
                <h2>Uploaded file(s) successfully.</h2>
                <p>
                    <a href="javascript:void(0)" @click="reset()">Upload again</a>
                </p>
            </div>
            <!--FAILED-->
            <div v-if="isFailed">
                <h2>Uploaded failed.</h2>
                <p>
                    <a href="javascript:void(0)" @click="reset()">Try again</a>
                </p>
                <pre>{{ uploadError }}</pre>
            </div>
        </div>
        <h2>Have {{ uploadedFiles.length }} file(s)</h2>
        <table class="table table-striped table-bordered">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Status</th>
                    <th>Download PDF</th>
                    <th></th>


                </tr>
            </thead>
            <tbody>
                <tr v-for="file in uploadedFiles" :key="uploadedFiles.id">
                    <td>{{file.name}}</td>
                    <p v-if="file.converted">
                        <h4 style="color:green">&#10003;</h4>

                    </p>
                    <td v-else>
                        <p v-if="isConverting">
                            <progress id="progress-bar" aria-label="Content loading…"></progress>

                        </p>
                        <p v-else>
                            <button class="btn btn-primary" :disabled="file.converted" @click="convertFileButton(file.name)">Convert</button>
                        </p>
                    </td>

                    <td v-if="file.converted">
                        <a :href="file.link">Download</a>
                    </td>
                    <td v-else>
                        <h6>No link ready yet</h6>
                    </td>
                    <td>
                        <button class="btn btn-danger" @click="deleteFileButton(file.name)" >Remove</button>
                    </td>


                </tr>
            </tbody>
        </table>
    </div>
</template>

<script>
    import { upload, getInputList, convertFile, deleteFile } from './file-upload.service';

    const STATUS_INITIAL = 0, STATUS_SAVING = 1, STATUS_SUCCESS = 2, STATUS_FAILED = 3, STATUS_CONVERTING = 4;

    export default {
        name: 'app',
        data() {
            return {
                uploadedFiles: [],
                uploadError: null,
                currentStatus: null,
                uploadFieldName: 'files'
            }
        },
        computed: {
            isInitial() {
                return this.currentStatus === STATUS_INITIAL;
            },
            isSaving() {
                return this.currentStatus === STATUS_SAVING;
            },
            isConverting() {
                return this.currentStatus === STATUS_CONVERTING;
            },
            isSuccess() {
                return this.currentStatus === STATUS_SUCCESS;
            },
            isFailed() {
                return this.currentStatus === STATUS_FAILED;
            }
        },
        methods: {
            reset() {
                // reset form to initial state
                this.currentStatus = STATUS_INITIAL;
                getInputList().then(s => this.uploadedFiles = s.data);
                this.uploadError = null;
            },
            save(formData) {
                // upload data to the server
                this.currentStatus = STATUS_SAVING;

                upload(formData)
                    .then(x => {
                        getInputList().then(s => this.uploadedFiles = s.data);
                        this.currentStatus = STATUS_SUCCESS;
                    })
                    .catch(err => {
                        console.log(err);
                        this.uploadError = err.response;
                        this.currentStatus = STATUS_FAILED;
                    });
            },
            filesChange(fieldName, fileList) {
                // handle file changes
                const formData = new FormData();

                if (!fileList.length) return;

                // append the files to FormData
                Array
                    .from(Array(fileList.length).keys())
                    .map(x => {
                        formData.append(fieldName, fileList[x], fileList[x].name);
                    });

                // save it
                this.save(formData);
            },
            deleteFileButton(fileName) {
                deleteFile(fileName)
                    .then(x => {
                        this.reset();
                    })
                    .catch(err => {
                        console.log(err);
                        this.uploadError = err.response;
                    });
            },
            convertFileButton(fileName) {
                this.currentStatus = STATUS_CONVERTING;
                convertFile(fileName)
                    .then(x => {
                        this.reset();
                    })
                    .catch(err => {
                        console.log(err);
                        this.uploadError = err.response;
                        this.currentStatus = STATUS_FAILED;
                    });
            }
        },
        mounted() {
            this.reset();
        },
    }

</script>

<style lang="scss">
    .dropbox {
        outline: 2px dashed grey; /* the dash box */
        outline-offset: -10px;
        background: lightcyan;
        color: dimgray;
        padding: 10px 10px;
        min-height: 200px; /* minimum height */
        position: relative;
        cursor: pointer;
    }

    .input-file {
        opacity: 0; /* invisible but it's there! */
        left:0;
        width: 100%;
        height: 200px;
        position: absolute;
        cursor: pointer;
    }

    .dropbox:hover {
        background: lightblue; /* when mouse over to the drop zone, change color */
    }

    .dropbox p {
        font-size: 1.2em;
        text-align: center;
        padding: 50px 0;
    }
    table td {
        vertical-align: middle;
    }
</style>