<template>
    <img alt="Vue logo" src="./assets/logo.png">
    <h1 >HTML to PDF Converter</h1>
    <UploadHtml />

</template>

<script>
    import UploadHtml from './components/UploadHtml.vue'

export default {
  name: 'App',
  components: {
      UploadHtml
  }
}
</script>

<style>
    #app {
        font-family: Avenir, Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
        text-align: center;
        color: #2c3e50;
        margin-top: 60px;
    }

</style>
