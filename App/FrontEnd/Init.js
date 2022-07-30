function getCookie(name) {
    var value = `; ${document.cookie}`;
    var parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}
Sentry.init({
    environment: "Testing",
    dsn: getCookie('SENTRY_DSN'),
    tracesSampleRate: 1.0,
    integrations: [new Sentry.Integrations.BrowserTracing()],
    sampleRate: 1.0,
});
