provider "kubernetes" {
  config_path = "~/.kube/config"
}

resource "kubernetes_namespace" "app" {
  metadata {
    name = var.namespace
  }
}

resource "kubernetes_deployment" "user" {
  metadata {
    name      = "user-service"
    namespace = kubernetes_namespace.app.metadata[0].name
  }

  spec {
    replicas = 2

    selector {
      match_labels = {
        app = "user-service"
      }
    }

    template {
      metadata {
        labels = {
          app = "user-service"
        }
      }

      spec {
        container {
          name  = "user-service"
          image = var.user_image

          port {
            container_port = 8080
          }

          readiness_probe {
            http_get {
              path = "/health"
              port = 8080
            }

            initial_delay_seconds = 3
            period_seconds        = 5
          }

          liveness_probe {
            http_get {
              path = "/health"
              port = 8080
            }

            initial_delay_seconds = 5
            period_seconds        = 10
          }

          resources {
            requests = {
              cpu    = "50m"
              memory = "32Mi"
            }

            limits = {
              cpu    = "250m"
              memory = "128Mi"
            }
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "user" {
  metadata {
    name      = "user-service"
    namespace = kubernetes_namespace.app.metadata[0].name
  }

  spec {
    selector = {
      app = "user-service"
    }

    port {
      port        = 8080
      target_port = 8080
    }
  }
}

resource "kubernetes_deployment" "order" {
  metadata {
    name      = "order-service"
    namespace = kubernetes_namespace.app.metadata[0].name
  }

  spec {
    replicas = 2

    selector {
      match_labels = {
        app = "order-service"
      }
    }

    template {
      metadata {
        labels = {
          app = "order-service"
        }
      }

      spec {
        container {
          name  = "order-service"
          image = var.order_image

          port {
            container_port = 8081
          }

          env {
            name  = "USER_SERVICE_URL"
            value = "http://user-service:8080"
          }

          readiness_probe {
            http_get {
              path = "/health"
              port = 8081
            }

            initial_delay_seconds = 3
            period_seconds        = 5
          }

          liveness_probe {
            http_get {
              path = "/health"
              port = 8081
            }

            initial_delay_seconds = 5
            period_seconds        = 10
          }

          resources {
            requests = {
              cpu    = "50m"
              memory = "64Mi"
            }

            limits = {
              cpu    = "300m"
              memory = "192Mi"
            }
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "order" {
  metadata {
    name      = "order-service"
    namespace = kubernetes_namespace.app.metadata[0].name
  }

  spec {
    selector = {
      app = "order-service"
    }

